# Self-contained unpackaged publish for portable folder deploy.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root 'artifacts\publish'

# Wipe prior publish output — leftover ReadyToRun / mixed binaries can crash
# WinUI on net10 (ComInterfaceEntry TypeLoadException at module init).
if (Test-Path $out) {
    Remove-Item $out -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $out | Out-Null

dotnet publish (Join-Path $root 'src\SimplestCalendar\SimplestCalendar.csproj') `
  -c Release `
  -p:Platform=x64 `
  -p:WindowsPackageType=None `
  -p:PublishReadyToRun=false `
  --self-contained true `
  -o $out

# Safety net: ensure compiled XAML + PRI landed (WinUI crashes without them).
$xbf = @(Get-ChildItem $out -Recurse -Filter '*.xbf' -ErrorAction SilentlyContinue)
$pri = @(Get-ChildItem $out -Filter '*.pri' -ErrorAction SilentlyContinue)
if ($xbf.Count -lt 1 -or $pri.Count -lt 1) {
    $bin = Join-Path $root 'src\SimplestCalendar\bin\x64\Release\net10.0-windows10.0.26100.0\win-x64'
    if (-not (Test-Path $bin)) { throw "Publish missing .xbf/.pri and bin fallback not found: $bin" }
    Copy-Item (Join-Path $bin '*.xbf') $out -Force -ErrorAction SilentlyContinue
    Copy-Item (Join-Path $bin '*.pri') $out -Force -ErrorAction SilentlyContinue
    $viewsOut = Join-Path $out 'Views'
    $viewsBin = Join-Path $bin 'Views'
    if (Test-Path $viewsBin) {
        New-Item -ItemType Directory -Force -Path $viewsOut | Out-Null
        Copy-Item (Join-Path $viewsBin '*.xbf') $viewsOut -Force
    }
    $xbf = @(Get-ChildItem $out -Recurse -Filter '*.xbf' -ErrorAction SilentlyContinue)
    $pri = @(Get-ChildItem $out -Filter '*.pri' -ErrorAction SilentlyContinue)
    if ($xbf.Count -lt 1 -or $pri.Count -lt 1) {
        throw "Publish output still missing WinUI .xbf/.pri after fallback copy."
    }
    Write-Host "Copied WinUI .xbf/.pri from bin into publish output." -ForegroundColor Yellow
}

Write-Host "Published to $out"
Write-Host "Run SimplestCalendar.exe from that folder; data/ is created beside the exe."
