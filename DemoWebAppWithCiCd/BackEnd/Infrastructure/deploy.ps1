
Set-StrictMode -Version "latest"
$ErrorActionPreference="Stop"

$ResourceGroup="rg-demo-webapp-with-cicd"
$Location="uksouth"
$PlanName="WebAppPlanName"
$NumOfWorkers=2
$WebAppName="MyDemoWebApi123"
$PlanSKu="FREE"
$ctx=Get-AzContext
New-AzResourceGroup -Name $ResourceGroup  -Location $Location -Force


Write-Host "Creating plan $PlanName"
az appservice plan create --name $PlanName --resource-group $ResourceGroup --sku $PlanSKu --number-of-workers $NumOfWorkers --subscription $ctx.Subscription.Id

Write-Host "Creating Web App $WebAppName"
az webapp create --name $WebAppName --plan $PlanName --resource-group $ResourceGroup --subscription $ctx.Subscription.Id

Write-Host "Setting configuration parameters"
$setting=@{}
$setting.Add("key001","value001")
$setting.Add("key002","value002")
Set-AzWebApp -ResourceGroupName $ResourceGroup -Name $WebAppName -AppSettings $setting


Write-Host "Removing all CORS sites"
az webapp cors remove --resource-group $ResourceGroup --name $WebAppName --allowed-origins


$corsUrls=@(
    "https://saustorageaccount001.z33.web.core.windows.net",
    "https://blah"
)
foreach ($corsUrl in $corsUrls) {
    Write-Host "Adding CORS url $corsUrl"
    az webapp cors add --resource-group $ResourceGroup --name $WebAppName --allowed-origins $corsUrl
}