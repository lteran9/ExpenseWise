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

resource "azurerm_mysql_flexible_server" "expensewise" {
  #checkov:skip=CKV_AZURE_94:Geo-redundant backups are out of scope for the demo (azure-deployment.md).
  #checkov:skip=CKV2_AZURE_56:VNet integration with public access disabled keeps the server private.
  name                         = "mysql-expensewise-${var.environment}-${local.name_suffix}"
  resource_group_name          = azurerm_resource_group.data.name
  location                     = azurerm_resource_group.data.location
  administrator_login          = "expensewiseadmin"
  administrator_password       = random_password.mysql_admin.result
  backup_retention_days        = var.mysql_backup_retention_days
  delegated_subnet_id          = azurerm_subnet.mysql.id
  private_dns_zone_id          = azurerm_private_dns_zone.mysql.id
  public_network_access        = "Disabled"
  sku_name                     = var.mysql_sku_name
  version                      = "8.0.21"
  geo_redundant_backup_enabled = false

  storage {
    size_gb           = var.mysql_storage_size_gb
    auto_grow_enabled = false
  }

  depends_on = [
    azurerm_private_dns_zone_virtual_network_link.mysql
  ]

  tags = local.tags
}

resource "azurerm_mysql_flexible_server_configuration" "require_tls" {
  name                = "require_secure_transport"
  resource_group_name = azurerm_resource_group.data.name
  server_name         = azurerm_mysql_flexible_server.expensewise.name
  value               = "ON"
}

resource "azurerm_mysql_flexible_database" "expensewise" {
  name                = "expensewise"
  resource_group_name = azurerm_resource_group.data.name
  server_name         = azurerm_mysql_flexible_server.expensewise.name
  charset             = "utf8mb4"
  collation           = "utf8mb4_unicode_ci"
}
