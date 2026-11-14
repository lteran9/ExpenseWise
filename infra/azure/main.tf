data "azurerm_client_config" "current" {}

data "azurerm_subscription" "current" {}

locals {
  name_suffix = random_string.resource_suffix.result

  tags = {
    application = "ExpenseWise"
    environment = var.environment
    managed_by  = "Terraform"
    owner       = var.owner_tag
  }

  mysql_zone_name = "private.mysql.database.azure.com"
}

resource "random_string" "resource_suffix" {
  length  = 5
  lower   = true
  upper   = false
  numeric = true
  special = false
}

resource "azurerm_resource_group" "network" {
  name     = "rg-expensewise-network-${var.environment}"
  location = var.location
  tags     = local.tags
}

resource "azurerm_resource_group" "data" {
  name     = "rg-expensewise-data-${var.environment}"
  location = var.location
  tags     = local.tags
}

resource "azurerm_resource_group" "shared" {
  name     = "rg-expensewise-shared-${var.environment}"
  location = var.location
  tags     = local.tags
}

resource "azurerm_resource_group" "apps" {
  name     = "rg-expensewise-apps-${var.environment}"
  location = var.location
  tags     = local.tags
}

resource "azurerm_log_analytics_workspace" "expensewise" {
  name                = "log-expensewise-${var.environment}"
  location            = azurerm_resource_group.shared.location
  resource_group_name = azurerm_resource_group.shared.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}
