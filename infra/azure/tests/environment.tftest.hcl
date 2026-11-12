# Credential-free checks of the environment configuration (see `make tf-test`):
#   terraform test -var-file=environments/dev.tfvars

mock_provider "azurerm" {
  mock_data "azurerm_client_config" {
    defaults = {
      tenant_id       = "00000000-0000-0000-0000-000000000001"
      object_id       = "00000000-0000-0000-0000-000000000002"
      subscription_id = "00000000-0000-0000-0000-000000000003"
      client_id       = "00000000-0000-0000-0000-000000000004"
    }
  }

  mock_data "azurerm_subscription" {
    defaults = {
      id              = "/subscriptions/00000000-0000-0000-0000-000000000003"
      subscription_id = "00000000-0000-0000-0000-000000000003"
    }
  }

  mock_resource "azurerm_resource_group" {
    defaults = {
      id = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test"
    }
  }

  mock_resource "azurerm_virtual_network" {
    defaults = {
      id = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.Network/virtualNetworks/vnet-test"
    }
  }

  mock_resource "azurerm_subnet" {
    defaults = {
      id = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.Network/virtualNetworks/vnet-test/subnets/snet-test"
    }
  }

  mock_resource "azurerm_private_dns_zone" {
    defaults = {
      id = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.Network/privateDnsZones/private.mysql.database.azure.com"
    }
  }

  mock_resource "azurerm_log_analytics_workspace" {
    defaults = {
      id = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.OperationalInsights/workspaces/log-test"
    }
  }

  mock_resource "azurerm_container_registry" {
    defaults = {
      id           = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.ContainerRegistry/registries/acrtest"
      login_server = "acrtest.azurecr.io"
    }
  }

  mock_resource "azurerm_key_vault" {
    defaults = {
      id = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.KeyVault/vaults/kv-test"
    }
  }

  mock_resource "azurerm_key_vault_secret" {
    defaults = {
      versionless_id = "https://kv-test.vault.azure.net/secrets/test"
      resource_id    = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.KeyVault/vaults/kv-test/secrets/test/versions/1"
    }
  }

  mock_resource "azurerm_user_assigned_identity" {
    defaults = {
      id           = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id-test"
      principal_id = "00000000-0000-0000-0000-000000000005"
      client_id    = "00000000-0000-0000-0000-000000000006"
    }
  }

  mock_resource "azurerm_mysql_flexible_server" {
    defaults = {
      id   = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.DBforMySQL/flexibleServers/mysql-test"
      fqdn = "mysql-test.private.mysql.database.azure.com"
    }
  }

  mock_resource "azurerm_container_app_environment" {
    defaults = {
      id             = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.App/managedEnvironments/cae-test"
      default_domain = "test.westus3.azurecontainerapps.io"
    }
  }

  mock_resource "azurerm_container_app" {
    defaults = {
      id = "/subscriptions/00000000-0000-0000-0000-000000000003/resourceGroups/rg-test/providers/Microsoft.App/containerApps/ca-test"
    }
  }
}

mock_provider "time" {
  mock_resource "time_static" {
    defaults = {
      rfc3339 = "2026-11-13T19:00:00Z"
    }
  }
}

variables {
  budget_contact_email = "alerts@example.com"
  image_tag            = "0123456789abcdef0123456789abcdef01234567"
}

run "environment" {
  command = apply

  assert {
    condition = (
      azurerm_resource_group.network.name == "rg-expensewise-network-${var.environment}" &&
      azurerm_resource_group.data.name == "rg-expensewise-data-${var.environment}" &&
      azurerm_resource_group.shared.name == "rg-expensewise-shared-${var.environment}" &&
      azurerm_resource_group.apps.name == "rg-expensewise-apps-${var.environment}"
    )
    error_message = "Resource groups must follow rg-expensewise-<category>-<environment> (ADR 0008)."
  }

  assert {
    condition = alltrue(concat(
      [for name in [
        azurerm_virtual_network.expensewise.resource_group_name,
        azurerm_subnet.container_apps.resource_group_name,
        azurerm_subnet.mysql.resource_group_name,
        azurerm_private_dns_zone.mysql.resource_group_name,
        azurerm_private_dns_zone_virtual_network_link.mysql.resource_group_name,
      ] : name == azurerm_resource_group.network.name],
      [for name in [
        azurerm_mysql_flexible_server.expensewise.resource_group_name,
        azurerm_mysql_flexible_server_configuration.require_tls.resource_group_name,
        azurerm_mysql_flexible_database.expensewise.resource_group_name,
      ] : name == azurerm_resource_group.data.name],
      [for name in [
        azurerm_container_registry.expensewise.resource_group_name,
        azurerm_key_vault.expensewise.resource_group_name,
        azurerm_log_analytics_workspace.expensewise.resource_group_name,
      ] : name == azurerm_resource_group.shared.name],
      [for name in [
        azurerm_container_app_environment.expensewise.resource_group_name,
        azurerm_user_assigned_identity.ui.resource_group_name,
        azurerm_user_assigned_identity.api.resource_group_name,
        azurerm_user_assigned_identity.migration.resource_group_name,
        azurerm_user_assigned_identity.database_user.resource_group_name,
        azurerm_container_app.api.resource_group_name,
        azurerm_container_app.ui.resource_group_name,
        azurerm_container_app_job.database_user.resource_group_name,
        azurerm_container_app_job.migration.resource_group_name,
        output.apps_resource_group_name,
      ] : name == azurerm_resource_group.apps.name],
    ))
    error_message = "Each resource must live in its lifecycle resource group (ADR 0008)."
  }

  assert {
    condition = alltrue([
      for tags in [
        azurerm_resource_group.network.tags,
        azurerm_resource_group.data.tags,
        azurerm_resource_group.shared.tags,
        azurerm_resource_group.apps.tags,
        azurerm_virtual_network.expensewise.tags,
        azurerm_mysql_flexible_server.expensewise.tags,
        azurerm_container_registry.expensewise.tags,
        azurerm_key_vault.expensewise.tags,
        azurerm_container_app_environment.expensewise.tags,
        azurerm_user_assigned_identity.api.tags,
        ] : tags == tomap({
          application = "ExpenseWise"
          environment = var.environment
          managed_by  = "Terraform"
          owner       = var.owner_tag
      })
    ])
    error_message = "Taggable resources must carry the application, environment, managed_by, and owner tags."
  }

  assert {
    condition = (
      azurerm_consumption_budget_subscription.monthly.subscription_id == data.azurerm_subscription.current.id &&
      azurerm_consumption_budget_subscription.monthly.amount == var.monthly_budget_usd &&
      one(azurerm_consumption_budget_subscription.monthly.time_period).start_date == "2026-11-01T00:00:00Z"
    )
    error_message = "The subscription budget must start on the first day of the month it is created."
  }

  assert {
    condition = (
      toset([for n in azurerm_consumption_budget_subscription.monthly.notification : n.threshold]) == toset([80, 100]) &&
      alltrue([for n in azurerm_consumption_budget_subscription.monthly.notification : n.contact_emails == tolist([var.budget_contact_email])])
    )
    error_message = "Budget alerts must notify the owner at 80% and 100% of actual spend."
  }

  assert {
    condition = (
      azurerm_mysql_flexible_server.expensewise.public_network_access == "Disabled" &&
      azurerm_mysql_flexible_server.expensewise.delegated_subnet_id == azurerm_subnet.mysql.id &&
      azurerm_mysql_flexible_server_configuration.require_tls.value == "ON"
    )
    error_message = "MySQL must be private on its delegated subnet and require TLS."
  }

  assert {
    condition     = !azurerm_container_registry.expensewise.admin_enabled && azurerm_key_vault.expensewise.rbac_authorization_enabled
    error_message = "Registry admin access must stay disabled and Key Vault must use Azure RBAC."
  }

  assert {
    condition = alltrue([
      for assignment in [
        azurerm_role_assignment.api_mysql_password,
        azurerm_role_assignment.api_jwt_signing_key,
        azurerm_role_assignment.migration_mysql_admin,
        azurerm_role_assignment.database_user_mysql_admin,
        azurerm_role_assignment.database_user_app_password,
      ] : assignment.role_definition_name == "Key Vault Secrets User" && strcontains(assignment.scope, "/secrets/")
    ])
    error_message = "Secret read access must be scoped to individual secrets."
  }

  assert {
    condition = (
      azurerm_role_assignment.deployer_secrets_officer.scope == azurerm_key_vault.expensewise.id &&
      azurerm_role_assignment.deployer_secrets_officer.role_definition_name == "Key Vault Secrets Officer" &&
      azurerm_role_assignment.deployer_secrets_officer.principal_id == data.azurerm_client_config.current.object_id
    )
    error_message = "The identity running Terraform needs Key Vault Secrets Officer on the vault to write secrets."
  }

  assert {
    condition = (
      time_sleep.deployer_rbac_propagation.create_duration == "60s" &&
      time_sleep.app_rbac_propagation.create_duration == "60s"
    )
    error_message = "Secrets and workloads must wait for Azure RBAC propagation."
  }

  assert {
    condition = (
      azurerm_container_app.api.template[0].container[0].image == "acrtest.azurecr.io/expensewise-api:${var.image_tag}" &&
      azurerm_container_app.ui.template[0].container[0].image == "acrtest.azurecr.io/expensewise-ui:${var.image_tag}" &&
      azurerm_container_app_job.migration.template[0].container[0].image == "acrtest.azurecr.io/expensewise-api:${var.image_tag}" &&
      azurerm_container_app_job.database_user.template[0].container[0].image == "acrtest.azurecr.io/expensewise-mysql-client:${var.image_tag}"
    )
    error_message = "Apps and jobs must start from the commit-tagged images in the registry."
  }

  assert {
    condition = (
      !azurerm_container_app.api.ingress[0].external_enabled &&
      azurerm_container_app.ui.ingress[0].external_enabled &&
      !azurerm_container_app.ui.ingress[0].allow_insecure_connections &&
      startswith(output.container_app_ui_url, "https://")
    )
    error_message = "Only the UI may be public, over HTTPS; the API stays internal."
  }

  assert {
    condition = (
      one(azurerm_container_app.api.template[0].container[0].liveness_probe).path == "/health/live" &&
      one(azurerm_container_app.api.template[0].container[0].readiness_probe).path == "/health/ready" &&
      one(azurerm_container_app.ui.template[0].container[0].liveness_probe).transport == "TCP"
    )
    error_message = "The API needs liveness and readiness probes and the UI a liveness probe."
  }
}

run "rejects_latest_image_tag" {
  command = plan

  variables {
    image_tag = "latest"
  }

  expect_failures = [var.image_tag]
}

run "rejects_invalid_budget_email" {
  command = plan

  variables {
    budget_contact_email = "not-an-email"
  }

  expect_failures = [var.budget_contact_email]
}
