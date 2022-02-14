Set-StrictMode -Version "latest"
$ErrorActionPreference="Stop"
$environment=$env:ENVIRONMENT
if ([string]::IsNullOrWhiteSpace($environment)){
    Write-Error -Message "The variable 'environment' was empty"
}

$ResourceGroup="rg-demo-backendwebapp-$environment"

$Location="uksouth"
$PlanName="WebAppPlanName"
$WebAppName="MyDemoWebApi123-$environment"

$StaticSiteStorageAccount="saustorageaccount001$environment"
$StaticSiteResourceGroup="rg-demo-staticwebsite-with-cicd"


