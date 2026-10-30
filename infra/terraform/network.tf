resource "azurerm_virtual_network" "expensewise" {
  name                = "vnet-expensewise-${var.environment}"
  location            = azurerm_resource_group.expensewise.location
  resource_group_name = azurerm_resource_group.expensewise.name
  address_space       = [var.virtual_network_cidr]
  tags                = local.tags
}

resource "azurerm_subnet" "container_apps" {
  name                 = "snet-containerapps"
  resource_group_name  = azurerm_resource_group.expensewise.name
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
  name                 = "snet-mysql"
  resource_group_name  = azurerm_resource_group.expensewise.name
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
  resource_group_name = azurerm_resource_group.expensewise.name
  tags                = local.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "mysql" {
  name                  = "link-expensewise-${var.environment}"
  private_dns_zone_name = azurerm_private_dns_zone.mysql.name
  resource_group_name   = azurerm_resource_group.expensewise.name
  virtual_network_id    = azurerm_virtual_network.expensewise.id
  registration_enabled  = false
  tags                  = local.tags
}

resource "azurerm_container_app_environment" "expensewise" {
  name                               = "cae-expensewise-${var.environment}"
  location                           = azurerm_resource_group.expensewise.location
  resource_group_name                = azurerm_resource_group.expensewise.name
  infrastructure_subnet_id           = azurerm_subnet.container_apps.id
  logs_destination                   = "log-analytics"
  log_analytics_workspace_id         = azurerm_log_analytics_workspace.expensewise.id
  zone_redundancy_enabled            = false
  infrastructure_resource_group_name = "ME_expensewise_${var.environment}"

  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }

  tags = local.tags
}
