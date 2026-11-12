resource "azurerm_container_registry" "expensewise" {
  #checkov:skip=CKV_AZURE_139:Basic SKU has no private endpoints; pulls require Microsoft Entra RBAC (ADR 0005).
  #checkov:skip=CKV_AZURE_163:Defender for Containers scanning is a paid plan this demo does not justify.
  #checkov:skip=CKV_AZURE_164:Content trust requires Premium SKU and Notary v1 is retired.
  #checkov:skip=CKV_AZURE_165:Geo-replication requires Premium SKU; the demo runs in one region.
  #checkov:skip=CKV_AZURE_166:Quarantine requires Premium SKU.
  #checkov:skip=CKV_AZURE_167:Untagged manifest retention requires Premium SKU.
  #checkov:skip=CKV_AZURE_233:Zone redundancy requires Premium SKU.
  #checkov:skip=CKV_AZURE_237:Dedicated data endpoints require Premium SKU.
  name                          = "acrexpensewisedev${local.name_suffix}"
  resource_group_name           = azurerm_resource_group.shared.name
  location                      = azurerm_resource_group.shared.location
  sku                           = "Basic"
  admin_enabled                 = false
  public_network_access_enabled = true
  tags                          = local.tags
}

resource "azurerm_user_assigned_identity" "ui" {
  name                = "id-expensewise-ui-${var.environment}"
  location            = azurerm_resource_group.apps.location
  resource_group_name = azurerm_resource_group.apps.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "api" {
  name                = "id-expensewise-api-${var.environment}"
  location            = azurerm_resource_group.apps.location
  resource_group_name = azurerm_resource_group.apps.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "migration" {
  name                = "id-expensewise-migration-${var.environment}"
  location            = azurerm_resource_group.apps.location
  resource_group_name = azurerm_resource_group.apps.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "database_user" {
  name                = "id-expensewise-dbuser-${var.environment}"
  location            = azurerm_resource_group.apps.location
  resource_group_name = azurerm_resource_group.apps.name
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
