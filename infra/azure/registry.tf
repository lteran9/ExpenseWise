resource "azurerm_container_registry" "expensewise" {
  #checkov:skip=CKV_AZURE_139:Basic SKU has no private endpoints; pulls require Microsoft Entra RBAC (ADR 0005).
  #checkov:skip=CKV_AZURE_163:Defender for Containers scanning is a paid plan this demo does not justify.
  #checkov:skip=CKV_AZURE_164:Content trust requires Premium SKU and Notary v1 is retired.
  #checkov:skip=CKV_AZURE_165:Geo-replication requires Premium SKU; the demo runs in one region.
  #checkov:skip=CKV_AZURE_166:Quarantine requires Premium SKU.
  #checkov:skip=CKV_AZURE_167:Untagged manifest retention requires Premium SKU.
  #checkov:skip=CKV_AZURE_233:Zone redundancy requires Premium SKU.
  #checkov:skip=CKV_AZURE_237:Dedicated data endpoints require Premium SKU.
  name                          = "acrexpensewisedev${local.name_suffix}"
  resource_group_name           = azurerm_resource_group.shared.name
  location                      = azurerm_resource_group.shared.location
  sku                           = "Basic"
  admin_enabled                 = false
  public_network_access_enabled = true
  tags                          = local.tags
}
