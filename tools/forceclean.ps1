<#
.Synopsis
Terminate all processes with a handle on the checkout directory and run 'git clean'.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$CheckoutPath  # path to repository
)

$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
Import-Module $ScriptDir\processutils.psm1

if ((Test-Path $CheckoutPath) -eq $false)
{
    Write-Host "Checkout path does not exist: $CheckoutPath"
    exit 1
}

Write-Host "Attempting to terminate processes with open handles on: $CheckoutPath"
try { Stop-ProcessesWithHandle -Path $CheckoutPath } catch {}

Write-Host "Running git clean"
Push-Location $CheckoutPath
& git clean -xxfd
