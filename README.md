# Readme

This repository contains a solution to an ANZ interview task.

## System components

* `AddReview` Azure Function - handles rating data validation and serialisation to CosmosDB
* `Cosmos DB` - persists raw ratings plus aggregated writers data
* `AggregateRating` Azure Function - implements `Authors` materialised view
* `TopAuthors` Azure Function - handles UI HTTP requests for top writers data

## Design justification

The solution is based on three key Azure components capable of low-latency, high-volume data input - Azure Functions, Azure Cosmos DB, and Azure Static Web App.

Azure Functions provide scaleable compute-on-demand experience with less infrastructure.

Azure Cosmos DB is a fully-managed database service with millisecond respond times which provides fast writes and reads.
Cosmos DB Change Feed feature allows to decouple raw rating serialisation and writers rating aggregation so that the API throughput is as fast as it can be.

The client is a single page application (SPA) deployed to an Azure static web site. This has the benefit of fast global reach and scale.

Together these three components ensure that the solution is capable of satifsying the initial 5 million rows per day requirement and is scalable at every node to allow for future growth.

## Caveats

* Change Feed aggregation may introduce delays between a data event and aggregated data update.
* The raw ratings data must be partitioned by an `Author` field. Additional Cosmos DB collections may be needed depending on further requirements.
* A cloud first solution may incur significant costs if let to scale uncontrollably.

## Further considerations

The following concerns were not addressed in the solution due to existing time constraints

* Authentication
* Data normalisation
* Better error handling in Azure Functions
* Allow for variable number of Top Writers to be requested from the UI (hardcoded to 10 at the moment)
* Unit testing
* Python team utilisation for more advanced analytics in Azure

## Prerequisites

If you don't have an Azure subscription, create a [free account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) before you begin.

In addition:

- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Node.js](https://nodejs.org/en/download/)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.2)

## Deploy

Before running any scripts make sure you are authenticated on AZ CLI using

```bash
az login
```

and have selected the Azure Subscription you want to use for the deploy:

```bash
az account list --output table
az account set --subscription "<YOUR SUBSCRIPTION NAME>"
```

Run `deploy.ps1` PowerShell script to build and deploy the solution to Azure.
The `$NAME` variable will be used to name a number of resources in your subscription. Please set it to a unique value before running the script.
A new app URL will be output to the console after a successful run.