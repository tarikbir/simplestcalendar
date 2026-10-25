# Self-contained unpackaged folder publish (fast launch; many DLLs — ship via installer).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root 'artifacts\publish'

if (Test-Path $out) {
    Remove-Item $out -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $out | Out-Null

dotnet publish (Join-Path $root 'src\SimplestCalendar\SimplestCalendar.csproj') `
  -c Release `
  -p:Platform=x64 `
  -p:RuntimeIdentifier=win-x64 `
  -p:WindowsPackageType=None `
  -p:PublishReadyToRun=false `
  -p:PublishTrimmed=false `
  --self-contained true `
  -o $out

# Safety net: WinUI needs .xbf/.pri beside the exe.
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

Get-ChildItem $out -Recurse -Filter '*.pdb' -ErrorAction SilentlyContinue | Remove-Item -Force

$exe = Join-Path $out 'SimplestCalendar.exe'
if (-not (Test-Path $exe)) { throw "Publish output missing: $exe" }
$vi = (Get-Item $exe).VersionInfo
Write-Host "Published to $out"
Write-Host ("FileVersion={0} ProductVersion={1}" -f $vi.FileVersion, $vi.ProductVersion)
Write-Host "For a single Setup.exe, run: ./scripts/build-installer.ps1"
