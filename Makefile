# Azure infrastructure (see docs/product/azure-deployment.md).
# Uses the terraform on PATH (latest release); set TERRAFORM to use another binary.

.PHONY: tf-fmt tf-validate tf-test tf-scan tf-check tf-init tf-plan tf-apply \
	azure-images azure-database-jobs azure-release

TERRAFORM ?= terraform
TF_ENV ?= dev
TF_DIR := infra/azure
IMAGE_TAG ?= $(shell git rev-parse HEAD)
TF_TARGETS ?=
RELEASE = TERRAFORM=$(TERRAFORM) $(TF_DIR)/scripts/release.sh $(IMAGE_TAG)
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

# Save the plan for review; tf-apply applies exactly that reviewed plan.
# TF_TARGETS limits the plan to the first-deployment stages (see the runbook).
tf-plan:
	umask 077 && $(TERRAFORM) -chdir=$(TF_DIR) plan -var-file=environments/$(TF_ENV).tfvars \
		-var=image_tag=$(IMAGE_TAG) $(addprefix -target=,$(TF_TARGETS)) -out=$(TF_ENV).tfplan

tf-apply:
	$(TERRAFORM) -chdir=$(TF_DIR) apply $(TF_ENV).tfplan

# Release stages run from the clean checkout of IMAGE_TAG; images build in Azure.
azure-images:
	$(RELEASE) images

azure-database-jobs:
	$(RELEASE) database-jobs

azure-release:
	$(RELEASE)
