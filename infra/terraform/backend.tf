terraform {
  required_version = "= 1.16.4"

  backend "azurerm" {}

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "= 4.81.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "= 3.8.1"
    }
  }
}

provider "azurerm" {
  features {}
}

provider "random" {}
