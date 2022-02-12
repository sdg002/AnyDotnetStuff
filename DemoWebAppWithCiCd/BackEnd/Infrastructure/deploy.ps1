param([string]$pathtozip)
Set-StrictMode -Version "latest"
$ErrorActionPreference="Stop"


Write-Host "Going to deploy the web app $pathtozip, "
Write-Host ("Value of environment={0}" -f $env:environment)

