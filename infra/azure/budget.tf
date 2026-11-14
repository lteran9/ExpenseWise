# Azure requires a budget to start on the first day of the current month, and
# changing the start date replaces the budget. Capture the creation time once.
resource "time_static" "budget_start" {}

resource "azurerm_consumption_budget_subscription" "monthly" {
  name            = "budget-expensewise-${var.environment}"
  subscription_id = data.azurerm_subscription.current.id
  amount          = var.monthly_budget_usd
  time_grain      = "Monthly"

  time_period {
    start_date = formatdate("YYYY-MM-01'T'00:00:00Z", time_static.budget_start.rfc3339)
  }

  notification {
    enabled        = true
    threshold      = 80
    operator       = "GreaterThan"
    threshold_type = "Actual"
    contact_emails = [var.budget_contact_email]
  }

  notification {
    enabled        = true
    threshold      = 100
    operator       = "GreaterThan"
    threshold_type = "Actual"
    contact_emails = [var.budget_contact_email]
  }
}
