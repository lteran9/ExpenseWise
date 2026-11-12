# Azure infrastructure (see docs/product/azure-deployment.md).
# Terraform is pinned to 1.16.4; point TERRAFORM at that binary if the default
# terraform on PATH is a different version.

.PHONY: tf-fmt tf-validate tf-test tf-scan tf-check tf-init tf-plan tf-apply

TERRAFORM ?= terraform
TF_ENV ?= dev
TF_DIR := infra/azure
CHECKOV_IMAGE := bridgecrew/checkov:3.3.24@sha256:395ab21f8dc5d457f05001598638a98d142a98e264d610650c084d0ef2e1b4a5

tf-fmt:
	$(TERRAFORM) fmt -check -recursive infra

tf-validate:
	for dir in infra/bootstrap $(TF_DIR); do \
		$(TERRAFORM) -chdir=$$dir init -backend=false -input=false > /dev/null && \
		$(TERRAFORM) -chdir=$$dir validate || exit 1; \
	done

# Mock providers: no Azure credentials needed.
tf-test:
	$(TERRAFORM) -chdir=$(TF_DIR) init -backend=false -input=false > /dev/null
	$(TERRAFORM) -chdir=$(TF_DIR) test -var-file=environments/$(TF_ENV).tfvars

tf-scan:
	docker run --rm -v "$(CURDIR)/infra:/src:ro" -w /src $(CHECKOV_IMAGE) \
		--directory /src --framework terraform --compact --quiet

tf-check: tf-fmt tf-validate tf-test tf-scan

tf-init:
	@test -n "$(TF_STATE_ACCOUNT)" || \
		{ echo "Set TF_STATE_ACCOUNT to the infra/bootstrap storage_account_name output."; exit 1; }
	$(TERRAFORM) -chdir=$(TF_DIR) init -reconfigure \
		-backend-config=environments/$(TF_ENV).backend.hcl \
		-backend-config=storage_account_name=$(TF_STATE_ACCOUNT)

# The staged rollout requires choosing both flags explicitly, so a plan never
# removes running workloads by falling back to the variable defaults. Save the
# plan for review; tf-apply applies exactly that reviewed plan.
tf-plan:
	@test "$(ENABLE_WORKLOADS)" = true -o "$(ENABLE_WORKLOADS)" = false || \
		{ echo "Set ENABLE_WORKLOADS=true or false for the deployment stage."; exit 1; }
	@test "$(ENABLE_PROBES)" = true -o "$(ENABLE_PROBES)" = false || \
		{ echo "Set ENABLE_PROBES=true or false for the deployment stage."; exit 1; }
	umask 077 && $(TERRAFORM) -chdir=$(TF_DIR) plan -var-file=environments/$(TF_ENV).tfvars \
		-var=enable_workloads=$(ENABLE_WORKLOADS) -var=enable_probes=$(ENABLE_PROBES) \
		-out=$(TF_ENV).tfplan

tf-apply:
	$(TERRAFORM) -chdir=$(TF_DIR) apply $(TF_ENV).tfplan
