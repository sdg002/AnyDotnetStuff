Set-StrictMode -Version "latest"
$ErrorActionPreference="Stop"

$ResourceGroup="rg-demo-staticwebsite-with-cicd"
$Location="uksouth"
$StaticSiteStorageAccount="saustorageaccount001"
$ContainerForStaticContent="`$web"
$ctx=Get-AzContext

Write-Host "Creating resource group $ResourceGroup at location $Location"
New-AzResourceGroup -Name $ResourceGroup  -Location $Location -Force

Write-Host "Creating storage account $StaticSiteStorageAccount"
az storage account create --name $StaticSiteStorageAccount --resource-group $ResourceGroup --location $Location --sku Standard_LRS  --subscription $ctx.Subscription.Id


#$stoAccount=Get-AzStorageAccount -ResourceGroupName $ResourceGroup -Name $StaticSiteStorageAccount
# $webContainer=Get-AzStorageContainer -Name $ContainerForStaticContent -Context $stoAccount.Context -ErrorAction Continue
# if ($null -ne $webContainer){
#     Write-Host "Deleting container $ContainerForStaticContent"
#     Remove-AzStorageContainer -Name $ContainerForStaticContent -Context $stoAccount.Context -Force
# }

Write-Host "Creating container $ContainerForStaticContent"
az storage container create --name $ContainerForStaticContent --resource-group $ResourceGroup --account-name $StaticSiteStorageAccount | Out-Null

Write-Host "Setting static web app properties"
az storage blob service-properties update --account-name $StaticSiteStorageAccount --static-website --404-document "error.html" --index-document "default.html" | Out-Null

Write-Host "Purging existing files in container $ContainerForStaticContent"
az storage blob delete-batch --account-name $StaticSiteStorageAccount --source $ContainerForStaticContent --pattern *.* 

$Sourcefolder=Join-Path -Path $PSScriptRoot -ChildPath "../src/public/"
Write-Host "Uploading files from $Sourcefolder"
az storage blob upload-batch --account-name $StaticSiteStorageAccount --source $Sourcefolder -d '$web'
