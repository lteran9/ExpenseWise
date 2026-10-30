output "resource_group_name" {
  description = "Resource group containing the ExpenseWise demo."
  value       = azurerm_resource_group.expensewise.name
}

output "container_registry_name" {
  description = "Azure Container Registry name."
  value       = azurerm_container_registry.expensewise.name
}

output "container_registry_login_server" {
  description = "Registry hostname used by the manual release script."
  value       = azurerm_container_registry.expensewise.login_server
}

output "key_vault_name" {
  description = "Key Vault holding generated workload credentials."
  value       = azurerm_key_vault.expensewise.name
}

output "mysql_server_name" {
  description = "Private MySQL Flexible Server name."
  value       = azurerm_mysql_flexible_server.expensewise.name
}

output "mysql_server_fqdn" {
  description = "Private MySQL Flexible Server hostname."
  value       = azurerm_mysql_flexible_server.expensewise.fqdn
}

output "workload_role_assignments" {
  description = "Role assignment scopes required by workload identities."
  value = concat(
    [
      for _, assignment in azurerm_role_assignment.acr_pull : {
        principal_id = assignment.principal_id
        scope        = assignment.scope
        role         = "AcrPull"
      }
    ],
    [
      {
        principal_id = azurerm_user_assigned_identity.api.principal_id
        scope        = azurerm_role_assignment.api_mysql_password.scope
        role         = "Key Vault Secrets User"
      },
      {
        principal_id = azurerm_user_assigned_identity.api.principal_id
        scope        = azurerm_role_assignment.api_jwt_signing_key.scope
        role         = "Key Vault Secrets User"
      },
      {
        principal_id = azurerm_user_assigned_identity.migration.principal_id
        scope        = azurerm_role_assignment.migration_mysql_admin.scope
        role         = "Key Vault Secrets User"
      },
      {
        principal_id = azurerm_user_assigned_identity.database_user.principal_id
        scope        = azurerm_role_assignment.database_user_mysql_admin.scope
        role         = "Key Vault Secrets User"
      },
      {
        principal_id = azurerm_user_assigned_identity.database_user.principal_id
        scope        = azurerm_role_assignment.database_user_app_password.scope
        role         = "Key Vault Secrets User"
      }
    ]
  )
}

output "container_app_ui_name" {
  description = "UI Container App name."
  value       = var.enable_workloads ? azurerm_container_app.ui[0].name : null
}

output "container_app_ui_url" {
  description = "Public HTTPS URL for the UI."
  value       = var.enable_workloads ? "https://${azurerm_container_app.ui[0].ingress[0].fqdn}" : null
}

output "container_app_api_name" {
  description = "Internal API Container App name."
  value       = var.enable_workloads ? azurerm_container_app.api[0].name : null
}

output "database_user_job_name" {
  description = "Manual job that establishes the least-privilege MySQL application user."
  value       = var.enable_workloads ? azurerm_container_app_job.database_user[0].name : null
}

output "migration_job_name" {
  description = "Manual EF Core migration job."
  value       = var.enable_workloads ? azurerm_container_app_job.migration[0].name : null
}
