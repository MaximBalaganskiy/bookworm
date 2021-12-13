$ErrorActionPreference = "Stop"
$NAME = "bookworm123456"
$LOCATION = 'australiasoutheast'

Write-Output "starting deployment: $NAME"
New-Item -Force ./logs -ItemType "directory"

Write-Output "creating resource group"
az group create -n $NAME -l $LOCATION -o json > ./logs/010-group-create.log

Write-Output "creating storage account"
az storage account create -n $NAME -g $NAME --sku Standard_LRS > ./logs/020-storage-account.log

Write-Output "creating cosmosdb account"
az cosmosdb create -g $NAME -n $NAME -o json > ./logs/030-cosmosdb-create.log

Write-Output "creating cosmosdb database"
az cosmosdb sql database create -g $NAME -a $NAME -n $NAME -o json > ./logs/040-cosmosdb-database-create.log

Write-Output "creating cosmosdb authors collection"
az cosmosdb sql container create -a $NAME -d $NAME -g $NAME -n Authors -p "/id" `
   --idx @author-indexing-policy.json `
   --throughput 1000 `
   -o json > ./logs/050-cosmosdb-authors-collection-create.log

Write-Output "creating cosmosdb ratings collection"
az cosmosdb sql container create -a $NAME -d $NAME -g $NAME -n Ratings ` -p "/Author" `
   --throughput 1000 `
   -o json > ./logs/060-cosmosdb-ratings-collection-create.log

Write-Output "creating function app"
az functionapp create -g $NAME -n $NAME `
   --functions-version 4 `
   --consumption-plan-location $LOCATION `
   --storage-account $NAME `
   -o json > ./logs/090-functionapp.log
az functionapp cors add -g $NAME -n $NAME --allowed-origins * >> ./logs/090-functionapp.log

Write-Output "adding app settings for connection strings"

Write-Output ". DatabaseName"
az functionapp config appsettings set --name $NAME `
   --resource-group $NAME `
   --settings DatabaseName=$NAME `
   -o json >> ./logs/090-functionapp.log

Write-Output ". ConnectionString"
$COSMOSDB_CONNECTIONSTRING = $(az cosmosdb keys list --type connection-strings -g $NAME --name $NAME --query 'connectionStrings[0].connectionString' -o tsv)
az functionapp config appsettings set --name $NAME `
   --resource-group $NAME `
   --settings ConnectionString=$COSMOSDB_CONNECTIONSTRING `
   -o json >> ./logs/090-functionapp.log

Write-Output "building function app"
dotnet publish ./Bookworm.Api --configuration Release 

Write-Output "creating zip file"
Compress-Archive -Path ./Bookworm.Api/bin/Release/net6.0/publish/* -DestinationPath ./publish.zip

Write-Output "deploying functions"
az functionapp deployment source config-zip -g $NAME -n $NAME --src ./publish.zip > ./logs/100-functionapp-deploy.log
Remove-Item -r ./publish.zip

Write-Output "building client app"
Set-Location ./bookworm-client
npm i 
npm run build
Set-Location ..

Write-Output "creating environment.json"
$TOP_AUTHORS_URL = $(az functionapp function show -g $NAME -n $NAME --function-name TopAuthors --query 'invokeUrlTemplate' -o tsv)
$TOP_AUTHORS_CODE = $(az functionapp function keys list -g $NAME -n $NAME --function-name TopAuthors --query 'default' -o tsv)
$ADD_RATING_URL = $(az functionapp function show -g $NAME -n $NAME --function-name AddRating --query 'invokeUrlTemplate' -o tsv)
$ADD_RATING_CODE = $(az functionapp function keys list -g $NAME -n $NAME --function-name AddRating --query 'default' -o tsv)
@{ topAuthorsUrl = "${TOP_AUTHORS_URL}?code=$TOP_AUTHORS_CODE"; addRatingUrl = "${ADD_RATING_URL}?code=$ADD_RATING_CODE" } | ConvertTo-Json | Out-File "./bookworm-client/dist/environment.json" -Encoding ascii

Write-Output "creating static web app"
az storage blob service-properties update --account-name $NAME --static-website --404-document index.html --index-document index.html > ./logs/110-staticapp-deploy.log
az storage blob upload-batch -s ./bookworm-client/dist -d '$web' --account-name $NAME >> ./logs/110-staticapp-deploy.log

Write-Output "the client is deployed at $(az storage account show -g $NAME -n $NAME --query primaryEndpoints.web)"