# Readme

This repository contains a solution to an ANZ interview task.

## Prerequisites

If you don't have an Azure subscription, create a [free account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) before you begin.

In addition:

- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Node.js](https://nodejs.org/en/download/)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.2)

## Components

* `Bookworm.Api` - Azure Functions Api
* `bookworm-client` - UI client

## Deploy

Run `deploy.ps1` PowerShell script to build and deploy the solution to Azure.
The script will output a new app URL to console logs after a successful run.