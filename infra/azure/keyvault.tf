resource "random_password" "jwt_signing" {
  length  = 64
  special = false
}

resource "azurerm_key_vault" "expensewise" {
  #checkov:skip=CKV_AZURE_42:Purge protection is off so the dev environment can be destroyed and recreated.
  #checkov:skip=CKV_AZURE_110:Purge protection is off so the dev environment can be destroyed and recreated.
  #checkov:skip=CKV_AZURE_109:Public endpoint is RBAC-only; private endpoints are deferred for cost (ADR 0005).
  #checkov:skip=CKV_AZURE_189:Public endpoint is required for Terraform and consumption Container Apps (ADR 0005).
  #checkov:skip=CKV2_AZURE_32:Private endpoints are deferred for cost (ADR 0005).
  name                          = "kv-expensewise-${var.environment}-${local.name_suffix}"
  location                      = azurerm_resource_group.shared.location
  resource_group_name           = azurerm_resource_group.shared.name
  tenant_id                     = data.azurerm_client_config.current.tenant_id
  sku_name                      = "standard"
  rbac_authorization_enabled    = true
  purge_protection_enabled      = false
  soft_delete_retention_days    = 7
  public_network_access_enabled = true
  tags                          = local.tags
}

resource "azurerm_key_vault_secret" "mysql_admin_password" {
  #checkov:skip=CKV_AZURE_41:Rotation is manual with terraform apply -replace (ADR 0006); no fixed expiry.
  name         = "mysql-admin-password"
  value        = random_password.mysql_admin.result
  key_vault_id = azurerm_key_vault.expensewise.id
  content_type = "ExpenseWise MySQL administrator password"
  tags         = local.tags
}

resource "azurerm_key_vault_secret" "mysql_application_password" {
  #checkov:skip=CKV_AZURE_41:Rotation is manual with terraform apply -replace (ADR 0006); no fixed expiry.
  name         = "mysql-application-password"
  value        = random_password.mysql_application.result
  key_vault_id = azurerm_key_vault.expensewise.id
  content_type = "ExpenseWise least-privilege application password"
  tags         = local.tags
}

resource "azurerm_key_vault_secret" "jwt_signing_key" {
  #checkov:skip=CKV_AZURE_41:Rotation is manual with terraform apply -replace (ADR 0006); no fixed expiry.
  name         = "jwt-signing-key"
  value        = random_password.jwt_signing.result
  key_vault_id = azurerm_key_vault.expensewise.id
  content_type = "ExpenseWise API signing key"
  tags         = local.tags
}
