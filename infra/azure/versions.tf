terraform {
  required_version = "= 1.16.4"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "= 4.81.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "= 3.8.1"
    }
    time = {
      source  = "hashicorp/time"
      version = "= 0.14.2"
    }
  }

  # Partial configuration: environments/<env>.backend.hcl plus the bootstrap
  # storage account name supplied at `terraform init` (see `make tf-init`).
  backend "azurerm" {}
}

# The subscription comes from ARM_SUBSCRIPTION_ID so no account identifiers are
# committed.
provider "azurerm" {
  features {}
}
