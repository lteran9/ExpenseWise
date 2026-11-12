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
  scope                = azurerm_key_vault_secret.mysql_application_password.resource_versionless_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "api_jwt_signing_key" {
  scope                = azurerm_key_vault_secret.jwt_signing_key.resource_versionless_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "migration_mysql_admin" {
  scope                = azurerm_key_vault_secret.mysql_admin_password.resource_versionless_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.migration.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "database_user_mysql_admin" {
  scope                = azurerm_key_vault_secret.mysql_admin_password.resource_versionless_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.database_user.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "database_user_app_password" {
  scope                = azurerm_key_vault_secret.mysql_application_password.resource_versionless_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.database_user.principal_id
  principal_type       = "ServicePrincipal"
}

# Azure RBAC assignments take a short time to propagate; Container Apps and jobs
# that pull images and resolve secrets before then fail to provision.
resource "time_sleep" "app_rbac_propagation" {
  create_duration = "60s"

  # Recreated secret role assignments need the same wait, so their IDs are tracked.
  triggers = {
    secret_role_assignments = join(",", [
      azurerm_role_assignment.api_mysql_password.id,
      azurerm_role_assignment.api_jwt_signing_key.id,
      azurerm_role_assignment.migration_mysql_admin.id,
      azurerm_role_assignment.database_user_mysql_admin.id,
      azurerm_role_assignment.database_user_app_password.id,
    ])
  }

  depends_on = [
    azurerm_role_assignment.acr_pull,
    azurerm_role_assignment.api_mysql_password,
    azurerm_role_assignment.api_jwt_signing_key,
    azurerm_role_assignment.migration_mysql_admin,
    azurerm_role_assignment.database_user_mysql_admin,
    azurerm_role_assignment.database_user_app_password,
  ]
}
