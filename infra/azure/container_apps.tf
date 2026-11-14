locals {
  api_image          = "${azurerm_container_registry.expensewise.login_server}/expensewise-api:${var.image_tag}"
  ui_image           = "${azurerm_container_registry.expensewise.login_server}/expensewise-ui:${var.image_tag}"
  mysql_client_image = "${azurerm_container_registry.expensewise.login_server}/expensewise-mysql-client:${var.image_tag}"
  app_database_user  = "expensewise_app"
}

resource "azurerm_container_app_environment" "expensewise" {
  name                               = "cae-expensewise-${var.environment}"
  location                           = azurerm_resource_group.apps.location
  resource_group_name                = azurerm_resource_group.apps.name
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

resource "azurerm_container_app" "api" {
  name                         = "ca-expensewise-api-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.expensewise.id
  resource_group_name          = azurerm_resource_group.apps.name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  registry {
    server   = azurerm_container_registry.expensewise.login_server
    identity = azurerm_user_assigned_identity.api.id
  }

  secret {
    name                = "mysql-application-password"
    key_vault_secret_id = azurerm_key_vault_secret.mysql_application_password.versionless_id
    identity            = azurerm_user_assigned_identity.api.id
  }

  secret {
    name                = "jwt-signing-key"
    key_vault_secret_id = azurerm_key_vault_secret.jwt_signing_key.versionless_id
    identity            = azurerm_user_assigned_identity.api.id
  }

  ingress {
    external_enabled = false
    target_port      = 8081
    transport        = "http"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    min_replicas = 1
    max_replicas = 1

    container {
      name   = "api"
      image  = local.api_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "Database__Host"
        value = azurerm_mysql_flexible_server.expensewise.fqdn
      }

      env {
        name  = "Database__Port"
        value = "3306"
      }

      env {
        name  = "Database__Name"
        value = azurerm_mysql_flexible_database.expensewise.name
      }

      env {
        name  = "Database__Username"
        value = local.app_database_user
      }

      env {
        name        = "Database__Password"
        secret_name = "mysql-application-password"
      }

      env {
        name        = "Authentication__SigningKey"
        secret_name = "jwt-signing-key"
      }

      env {
        name  = "Database__SslMode"
        value = "Required"
      }

      liveness_probe {
        transport = "HTTP"
        port      = 8081
        path      = "/health/live"

        initial_delay    = 20
        interval_seconds = 10
      }

      # Readiness checks the database as the application user, so the
      # database-user job must have run before the API is first created.
      readiness_probe {
        transport = "HTTP"
        port      = 8081
        path      = "/health/ready"

        initial_delay    = 30
        interval_seconds = 15
      }
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }

  depends_on = [
    time_sleep.app_rbac_propagation,
    azurerm_mysql_flexible_server_configuration.require_tls
  ]

  tags = local.tags
}

resource "azurerm_container_app" "ui" {
  name                         = "ca-expensewise-ui-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.expensewise.id
  resource_group_name          = azurerm_resource_group.apps.name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.ui.id]
  }

  registry {
    server   = azurerm_container_registry.expensewise.login_server
    identity = azurerm_user_assigned_identity.ui.id
  }

  ingress {
    external_enabled           = true
    target_port                = 8080
    transport                  = "http"
    allow_insecure_connections = false

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    min_replicas = 1
    max_replicas = 1

    container {
      name   = "ui"
      image  = local.ui_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ExpenseWiseApi__BaseUrl"
        value = "https://${azurerm_container_app.api.name}.internal.${azurerm_container_app_environment.expensewise.default_domain}/"
      }

      liveness_probe {
        transport = "TCP"
        port      = 8080

        initial_delay    = 20
        interval_seconds = 10
      }
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }

  depends_on = [
    time_sleep.app_rbac_propagation
  ]

  tags = local.tags
}

resource "azurerm_container_app_job" "database_user" {
  name                         = "job-expensewise-dbuser-${var.environment}"
  location                     = azurerm_resource_group.apps.location
  resource_group_name          = azurerm_resource_group.apps.name
  container_app_environment_id = azurerm_container_app_environment.expensewise.id
  workload_profile_name        = "Consumption"
  replica_timeout_in_seconds   = 300
  replica_retry_limit          = 1
  manual_trigger_config {
    parallelism              = 1
    replica_completion_count = 1
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.database_user.id]
  }

  registry {
    server   = azurerm_container_registry.expensewise.login_server
    identity = azurerm_user_assigned_identity.database_user.id
  }

  secret {
    name                = "mysql-admin-password"
    key_vault_secret_id = azurerm_key_vault_secret.mysql_admin_password.versionless_id
    identity            = azurerm_user_assigned_identity.database_user.id
  }

  secret {
    name                = "mysql-application-password"
    key_vault_secret_id = azurerm_key_vault_secret.mysql_application_password.versionless_id
    identity            = azurerm_user_assigned_identity.database_user.id
  }

  template {
    container {
      name    = "database-user"
      image   = local.mysql_client_image
      cpu     = 0.25
      memory  = "0.5Gi"
      command = ["/bin/sh", "-c"]
      args = [
        "set -eu\nsed -e \"s|__MYSQL_APP_USER__|${local.app_database_user}|g\" -e \"s|__MYSQL_DATABASE__|$MYSQL_DATABASE|g\" -e \"s|__MYSQL_APP_PASSWORD__|$MYSQL_APP_PASSWORD|g\" /scripts/create-app-user.sql | MYSQL_PWD=\"$MYSQL_ADMIN_PASSWORD\" mysql --ssl-mode=REQUIRED --protocol=TCP --host=\"$MYSQL_HOST\" --port=3306 --user=\"$MYSQL_ADMIN_USER\""
      ]

      env {
        name  = "MYSQL_HOST"
        value = azurerm_mysql_flexible_server.expensewise.fqdn
      }

      env {
        name  = "MYSQL_ADMIN_USER"
        value = azurerm_mysql_flexible_server.expensewise.administrator_login
      }

      env {
        name        = "MYSQL_ADMIN_PASSWORD"
        secret_name = "mysql-admin-password"
      }

      env {
        name  = "MYSQL_APP_USER"
        value = local.app_database_user
      }

      env {
        name        = "MYSQL_APP_PASSWORD"
        secret_name = "mysql-application-password"
      }

      env {
        name  = "MYSQL_DATABASE"
        value = azurerm_mysql_flexible_database.expensewise.name
      }
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }

  depends_on = [
    time_sleep.app_rbac_propagation
  ]

  tags = local.tags
}

resource "azurerm_container_app_job" "migration" {
  name                         = "job-expensewise-migration-${var.environment}"
  location                     = azurerm_resource_group.apps.location
  resource_group_name          = azurerm_resource_group.apps.name
  container_app_environment_id = azurerm_container_app_environment.expensewise.id
  workload_profile_name        = "Consumption"
  replica_timeout_in_seconds   = 900
  replica_retry_limit          = 0
  manual_trigger_config {
    parallelism              = 1
    replica_completion_count = 1
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.migration.id]
  }

  registry {
    server   = azurerm_container_registry.expensewise.login_server
    identity = azurerm_user_assigned_identity.migration.id
  }

  secret {
    name                = "mysql-admin-password"
    key_vault_secret_id = azurerm_key_vault_secret.mysql_admin_password.versionless_id
    identity            = azurerm_user_assigned_identity.migration.id
  }

  template {
    container {
      name    = "migration"
      image   = local.api_image
      cpu     = 0.25
      memory  = "0.5Gi"
      command = ["dotnet", "Api.dll"]
      args    = ["--migrate"]

      env {
        name  = "Database__Host"
        value = azurerm_mysql_flexible_server.expensewise.fqdn
      }

      env {
        name  = "Database__Port"
        value = "3306"
      }

      env {
        name  = "Database__Name"
        value = azurerm_mysql_flexible_database.expensewise.name
      }

      env {
        name  = "Database__Username"
        value = azurerm_mysql_flexible_server.expensewise.administrator_login
      }

      env {
        name        = "Database__Password"
        secret_name = "mysql-admin-password"
      }

      env {
        name  = "Database__SslMode"
        value = "Required"
      }
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }

  depends_on = [
    time_sleep.app_rbac_propagation,
    azurerm_mysql_flexible_server_configuration.require_tls
  ]

  tags = local.tags
}
