output "resource_group_names" {
  description = "Resource groups containing the ExpenseWise demo, by category."
  value = {
    network = azurerm_resource_group.network.name
    data    = azurerm_resource_group.data.name
    shared  = azurerm_resource_group.shared.name
    apps    = azurerm_resource_group.apps.name
  }
}

output "apps_resource_group_name" {
  description = "Resource group containing the Container Apps environment, apps, jobs, and workload identities."
  value       = azurerm_resource_group.apps.name
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

output "container_app_ui_name" {
  description = "UI Container App name."
  value       = azurerm_container_app.ui.name
}

output "container_app_ui_url" {
  description = "Public HTTPS URL for the UI."
  value       = "https://${azurerm_container_app.ui.ingress[0].fqdn}"
}

output "container_app_api_name" {
  description = "Internal API Container App name."
  value       = azurerm_container_app.api.name
}

output "database_user_job_name" {
  description = "Manual job that establishes the least-privilege MySQL application user."
  value       = azurerm_container_app_job.database_user.name
}

output "migration_job_name" {
  description = "Manual EF Core migration job."
  value       = azurerm_container_app_job.migration.name
}
