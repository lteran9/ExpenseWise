# ADR 0011: Use the latest Terraform CLI release

- Status: Proposed; would amend [ADR 0009](0009-terraform-bootstrap-and-environment-roots.md)
- Date: 2026-11-13
- Amends: the Terraform CLI pin in the Decision and Consequences of [ADR 0009](0009-terraform-bootstrap-and-environment-roots.md). Provider version pins and lock files are unchanged.

## Context

ADR 0009 pins the Terraform CLI to 1.16.4 in both roots (`infra/azure` and `infra/bootstrap`) and in the CI `infrastructure` job. The owner asked to stop pinning the CLI and use the latest release. The exact AzureRM, random, and time provider versions and the committed `.terraform.lock.hcl` files stay as they are, so provider behavior does not change unless a provider is deliberately upgraded.

## Decision

- Remove the `required_version` constraint from `infra/azure` and `infra/bootstrap`. Both roots run with the `terraform` on `PATH`; `TERRAFORM=<path>` in the Make targets and the release script selects another binary.
- The CI `infrastructure` job installs the latest Terraform CLI by omitting the `terraform_version` input of `hashicorp/setup-terraform`.
- Keep the exact provider versions and the committed dependency lock files for both roots.

## Consequences

- A new Terraform CLI release can change formatting, validation, or test behavior without a repository change, so the CI `infrastructure` job can fail on unchanged code.
- No minimum CLI version is enforced, so an older binary runs until it reaches syntax or features it does not support.
- The owner's reviewed plan remains the gate before any apply, as ADR 0009 requires.
