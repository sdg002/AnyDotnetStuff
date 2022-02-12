Set-StrictMode -Version "latest"
$ErrorActionPreference="Stop"
$environment=$env:ENVIRONMENT
if ([string]::IsNullOrWhiteSpace($environment)){
    Write-Error -Message "The variable 'environment' was empty"
}

$ResourceGroup="rg-$environment-demo-webapp-with-cicd"

$Location="uksouth"
$PlanName="WebAppPlanName"

$StaticSiteStorageAccount="saustorageaccount001$environment"
$StaticSiteResourceGroup="rg-demo-staticwebsite-with-cicd"


