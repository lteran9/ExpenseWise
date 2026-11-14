# One-time bootstrap: the storage account that holds every ExpenseWise
# environment's Terraform state. It uses local state because it cannot store
# its own state before it exists. Keep terraform.tfstate from this directory
# somewhere private (it holds identifiers only); the resource group is
# delete-locked.

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "= 4.81.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "= 3.8.1"
    }
  }
}

# The subscription comes from ARM_SUBSCRIPTION_ID. Shared keys are disabled, so
# storage data-plane calls must use Microsoft Entra ID.
provider "azurerm" {
  storage_use_azuread = true
  features {}
}

variable "location" {
  description = "Region for the state storage account."
  type        = string
  default     = "westus3"
}

variable "owner" {
  description = "Owner tag value."
  type        = string
  default     = "Luis Teran"
}

variable "environments" {
  description = "Environments that each get an isolated state container."
  type        = set(string)
  default     = ["dev"]
}

data "azurerm_client_config" "current" {}

locals {
  tags = {
    application = "ExpenseWise"
    environment = "shared"
    managed_by  = "Terraform"
    owner       = var.owner
  }
}

resource "random_string" "suffix" {
  length  = 8
  lower   = true
  upper   = false
  numeric = true
  special = false
}

resource "azurerm_resource_group" "state" {
  name     = "rg-expensewise-tfstate"
  location = var.location
  tags     = local.tags
}

resource "azurerm_storage_account" "state" {
  #checkov:skip=CKV_AZURE_33:Queue service is unused; only blob storage holds state.
  #checkov:skip=CKV_AZURE_35:Terraform runs from the owner's workstation; access is Microsoft Entra RBAC only.
  #checkov:skip=CKV_AZURE_59:Public endpoint required for workstation Terraform; shared keys are disabled.
  #checkov:skip=CKV2_AZURE_1:Microsoft-managed keys are sufficient for demo state.
  #checkov:skip=CKV2_AZURE_33:A private endpoint is not justified for the demo state store.
  name                              = "stexpensewisetf${random_string.suffix.result}"
  resource_group_name               = azurerm_resource_group.state.name
  location                          = azurerm_resource_group.state.location
  account_kind                      = "StorageV2"
  account_tier                      = "Standard"
  account_replication_type          = "GRS"
  min_tls_version                   = "TLS1_2"
  https_traffic_only_enabled        = true
  shared_access_key_enabled         = false
  default_to_oauth_authentication   = true
  allow_nested_items_to_be_public   = false
  infrastructure_encryption_enabled = true
  tags                              = local.tags

  blob_properties {
    versioning_enabled = true

    delete_retention_policy {
      days = 30
    }

    container_delete_retention_policy {
      days = 30
    }
  }

  sas_policy {
    expiration_period = "00.01:00:00"
  }
}

# One container per environment isolates state and lets later automation
# identities be granted access to a single environment.
resource "azurerm_storage_container" "state" {
  #checkov:skip=CKV2_AZURE_21:Blob read logging is not needed for demo state.
  for_each = var.environments

  name                  = "tfstate-${each.key}"
  storage_account_id    = azurerm_storage_account.state.id
  container_access_type = "private"
}

resource "azurerm_role_assignment" "state_operator" {
  for_each = azurerm_storage_container.state

  scope                = each.value.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_management_lock" "state" {
  name       = "expensewise-tfstate-protect"
  scope      = azurerm_resource_group.state.id
  lock_level = "CanNotDelete"
  notes      = "Holds Terraform state for every ExpenseWise environment."

  lifecycle {
    prevent_destroy = true
  }
}

output "resource_group_name" {
  description = "State resource group (matches infra/azure/environments/*.backend.hcl)."
  value       = azurerm_resource_group.state.name
}

output "storage_account_name" {
  description = "Pass to `make tf-init TF_STATE_ACCOUNT=...`."
  value       = azurerm_storage_account.state.name
}

output "containers" {
  description = "State container per environment."
  value       = { for env, container in azurerm_storage_container.state : env => container.name }
}
