# Full automated verification for Simplest Calendar MVP.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host '== Test + coverage (>=50% line) ==' -ForegroundColor Cyan
dotnet test tests/SimplestCalendar.Core.Tests/SimplestCalendar.Core.Tests.csproj `
  -p:CollectCoverage=true `
  -p:CoverletOutputFormat=cobertura `
  -p:Threshold=50 `
  -p:ThresholdType=line

Write-Host '== Build app (x64 Debug) ==' -ForegroundColor Cyan
dotnet build src/SimplestCalendar/SimplestCalendar.csproj -c Debug -p:Platform=x64

Write-Host '== Publish self-contained ==' -ForegroundColor Cyan
& "$PSScriptRoot\publish.ps1"

$exe = Join-Path $root 'artifacts\publish\SimplestCalendar.exe'
if (-not (Test-Path $exe)) { throw "Publish output missing: $exe" }

Write-Host '== Launch smoke (5s) ==' -ForegroundColor Cyan
$p = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -PassThru
Start-Sleep -Seconds 5
if ($p.HasExited) { throw "App exited early with code $($p.ExitCode)" }
Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
Write-Host 'VERIFY_OK' -ForegroundColor Green
