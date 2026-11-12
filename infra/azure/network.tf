resource "azurerm_virtual_network" "expensewise" {
  name                = "vnet-expensewise-${var.environment}"
  location            = azurerm_resource_group.network.location
  resource_group_name = azurerm_resource_group.network.name
  address_space       = [var.virtual_network_cidr]
  tags                = local.tags
}

resource "azurerm_subnet" "container_apps" {
  #checkov:skip=CKV2_AZURE_31:Exposure is set by Container Apps ingress (internal API, HTTPS-only UI); no NSG yet.
  name                 = "snet-containerapps"
  resource_group_name  = azurerm_resource_group.network.name
  virtual_network_name = azurerm_virtual_network.expensewise.name
  address_prefixes     = [var.container_apps_subnet_cidr]

  delegation {
    name = "container-apps"

    service_delegation {
      name = "Microsoft.App/environments"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action"
      ]
    }
  }
}

resource "azurerm_subnet" "mysql" {
  #checkov:skip=CKV2_AZURE_31:Delegated subnet holds only the private MySQL server; no NSG yet.
  name                 = "snet-mysql"
  resource_group_name  = azurerm_resource_group.network.name
  virtual_network_name = azurerm_virtual_network.expensewise.name
  address_prefixes     = [var.mysql_subnet_cidr]

  delegation {
    name = "mysql-flexible-server"

    service_delegation {
      name = "Microsoft.DBforMySQL/flexibleServers"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action"
      ]
    }
  }
}

resource "azurerm_private_dns_zone" "mysql" {
  name                = local.mysql_zone_name
  resource_group_name = azurerm_resource_group.network.name
  tags                = local.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "mysql" {
  name                  = "link-expensewise-${var.environment}"
  private_dns_zone_name = azurerm_private_dns_zone.mysql.name
  resource_group_name   = azurerm_resource_group.network.name
  virtual_network_id    = azurerm_virtual_network.expensewise.id
  registration_enabled  = false
  tags                  = local.tags
}
