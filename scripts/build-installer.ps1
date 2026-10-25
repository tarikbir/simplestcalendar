# Publish folder build + Inno Setup installer (single Setup.exe for users).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

& "$PSScriptRoot\publish.ps1"

function Find-Iscc {
    @(
        Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"
        Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 7\ISCC.exe"
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
        "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe"
        "$env:ProgramFiles\Inno Setup 7\ISCC.exe"
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1
}

$iscc = Find-Iscc
if (-not $iscc) {
    Write-Host "Inno Setup not found. Installing via winget..." -ForegroundColor Yellow
    winget install --id JRSoftware.InnoSetup -e --accept-package-agreements --accept-source-agreements
    $iscc = Find-Iscc
}

if (-not $iscc) {
    throw "ISCC.exe still not found. Install Inno Setup 6 from https://jrsoftware.org/isinfo.php then re-run."
}

$csproj = Join-Path $root 'src\SimplestCalendar\SimplestCalendar.csproj'
$verMatch = Select-String -Path $csproj -Pattern '<Version>([^<]+)</Version>' | Select-Object -First 1
$appVersion = if ($verMatch) { $verMatch.Matches[0].Groups[1].Value } else { '0.3.1' }

$outDir = Join-Path $root 'artifacts\installer'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

& $iscc "/DAppVersion=$appVersion" (Join-Path $root 'installer\SimplestCalendar.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compile failed with exit $LASTEXITCODE" }

$setup = Get-ChildItem $outDir -Filter "SimplestCalendar-Setup-*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host "Installer: $($setup.FullName)" -ForegroundColor Green
Write-Host ("Size: {0:N1} MB" -f ($setup.Length / 1MB))
