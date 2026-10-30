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

resource "random_password" "mysql_admin" {
  length      = 32
  special     = false
  min_lower   = 1
  min_upper   = 1
  min_numeric = 1
}

resource "random_password" "mysql_application" {
  length      = 40
  special     = false
  min_lower   = 1
  min_upper   = 1
  min_numeric = 1
}

resource "random_password" "jwt_signing" {
  length  = 64
  special = false
}

resource "azurerm_resource_group" "expensewise" {
  name     = var.resource_group_name
  location = var.location
  tags     = local.tags
}

resource "azurerm_consumption_budget_resource_group" "monthly" {
  name              = "budget-expensewise-${var.environment}"
  resource_group_id = azurerm_resource_group.expensewise.id
  amount            = var.monthly_budget_usd
  time_grain        = "Monthly"

  time_period {
    start_date = "2026-10-01T00:00:00Z"
    end_date   = "2036-10-01T00:00:00Z"
  }

  notification {
    enabled        = true
    threshold      = 80
    operator       = "GreaterThan"
    threshold_type = "Actual"
    contact_emails = [var.budget_contact_email]
  }

  notification {
    enabled        = true
    threshold      = 100
    operator       = "GreaterThan"
    threshold_type = "Actual"
    contact_emails = [var.budget_contact_email]
  }
}

resource "azurerm_log_analytics_workspace" "expensewise" {
  name                = "log-expensewise-${var.environment}"
  location            = azurerm_resource_group.expensewise.location
  resource_group_name = azurerm_resource_group.expensewise.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}
