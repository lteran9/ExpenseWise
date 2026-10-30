resource "azurerm_container_registry" "expensewise" {
  name                          = "acrexpensewisedev${local.name_suffix}"
  resource_group_name           = azurerm_resource_group.expensewise.name
  location                      = azurerm_resource_group.expensewise.location
  sku                           = "Basic"
  admin_enabled                 = false
  public_network_access_enabled = true
  tags                          = local.tags
}

resource "azurerm_key_vault" "expensewise" {
  name                          = "kv-expensewise-${var.environment}-${local.name_suffix}"
  location                      = azurerm_resource_group.expensewise.location
  resource_group_name           = azurerm_resource_group.expensewise.name
  tenant_id                     = data.azurerm_client_config.current.tenant_id
  sku_name                      = "standard"
  rbac_authorization_enabled    = true
  purge_protection_enabled      = false
  soft_delete_retention_days    = 7
  public_network_access_enabled = true
  tags                          = local.tags
}

data "azurerm_client_config" "current" {}

resource "azurerm_key_vault_secret" "mysql_admin_password" {
  name         = "mysql-admin-password"
  value        = random_password.mysql_admin.result
  key_vault_id = azurerm_key_vault.expensewise.id
  content_type = "ExpenseWise MySQL administrator password"
  tags         = local.tags
}

resource "azurerm_key_vault_secret" "mysql_application_password" {
  name         = "mysql-application-password"
  value        = random_password.mysql_application.result
  key_vault_id = azurerm_key_vault.expensewise.id
  content_type = "ExpenseWise least-privilege application password"
  tags         = local.tags
}

resource "azurerm_key_vault_secret" "jwt_signing_key" {
  name         = "jwt-signing-key"
  value        = random_password.jwt_signing.result
  key_vault_id = azurerm_key_vault.expensewise.id
  content_type = "ExpenseWise API signing key"
  tags         = local.tags
}

resource "azurerm_user_assigned_identity" "ui" {
  name                = "id-expensewise-ui-${var.environment}"
  location            = azurerm_resource_group.expensewise.location
  resource_group_name = azurerm_resource_group.expensewise.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "api" {
  name                = "id-expensewise-api-${var.environment}"
  location            = azurerm_resource_group.expensewise.location
  resource_group_name = azurerm_resource_group.expensewise.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "migration" {
  name                = "id-expensewise-migration-${var.environment}"
  location            = azurerm_resource_group.expensewise.location
  resource_group_name = azurerm_resource_group.expensewise.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "database_user" {
  name                = "id-expensewise-dbuser-${var.environment}"
  location            = azurerm_resource_group.expensewise.location
  resource_group_name = azurerm_resource_group.expensewise.name
  tags                = local.tags
}

locals {
  acr_pull_identity_ids = {
    ui            = azurerm_user_assigned_identity.ui.principal_id
    api           = azurerm_user_assigned_identity.api.principal_id
    migration     = azurerm_user_assigned_identity.migration.principal_id
    database_user = azurerm_user_assigned_identity.database_user.principal_id
  }
}

resource "azurerm_role_assignment" "acr_pull" {
  for_each             = local.acr_pull_identity_ids
  scope                = azurerm_container_registry.expensewise.id
  role_definition_name = "AcrPull"
  principal_id         = each.value
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "api_mysql_password" {
  scope                = azurerm_key_vault_secret.mysql_application_password.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "api_jwt_signing_key" {
  scope                = azurerm_key_vault_secret.jwt_signing_key.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "migration_mysql_admin" {
  scope                = azurerm_key_vault_secret.mysql_admin_password.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.migration.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "database_user_mysql_admin" {
  scope                = azurerm_key_vault_secret.mysql_admin_password.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.database_user.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "database_user_app_password" {
  scope                = azurerm_key_vault_secret.mysql_application_password.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.database_user.principal_id
  principal_type       = "ServicePrincipal"
}
