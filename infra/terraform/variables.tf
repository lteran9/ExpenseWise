variable "location" {
  description = "Azure region for the demo environment."
  type        = string
}

variable "environment" {
  description = "Deployment environment name."
  type        = string
}

variable "resource_group_name" {
  description = "Dedicated resource group owned by ExpenseWise."
  type        = string
}

variable "virtual_network_cidr" {
  description = "Address space for the dedicated ExpenseWise virtual network."
  type        = string
}

variable "container_apps_subnet_cidr" {
  description = "Delegated subnet for the Container Apps environment; must be at least /23."
  type        = string
}

variable "mysql_subnet_cidr" {
  description = "Delegated subnet for the private MySQL Flexible Server."
  type        = string
}

variable "mysql_sku_name" {
  description = "MySQL Flexible Server burstable SKU."
  type        = string
}

variable "mysql_storage_size_gb" {
  description = "MySQL Flexible Server storage size in GiB."
  type        = number
}

variable "mysql_backup_retention_days" {
  description = "MySQL backup retention in days."
  type        = number
}

variable "monthly_budget_usd" {
  description = "Monthly resource-group budget amount in USD; the budget alerts but does not stop spend."
  type        = number
}

variable "budget_contact_email" {
  description = "Owner email for budget alerts. Set in untracked dev.local.tfvars."
  type        = string
  sensitive   = true

  validation {
    condition     = can(regex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$", var.budget_contact_email))
    error_message = "budget_contact_email must be a valid email address."
  }
}

variable "owner_tag" {
  description = "Owner tag value; set to a non-personal team or portfolio label."
  type        = string
}

variable "enable_workloads" {
  description = "Create Container Apps and jobs after the identity and registry permissions have propagated."
  type        = bool
  default     = false
}

variable "enable_probes" {
  description = "Enable application health probes after deploying the real application images."
  type        = bool
  default     = false
}
