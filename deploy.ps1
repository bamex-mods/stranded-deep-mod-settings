$ErrorActionPreference = "Stop"
$GameRoot = "F:\SteamLibrary\steamapps\common\Stranded Deep"
$Plugins = Join-Path $GameRoot "BepInEx\plugins"
$PluginDir = Join-Path $Plugins "StrandedDeepModSettings"
$BuildDll = Join-Path $PSScriptRoot "out\StrandedDeepModSettings.dll"
$RuntimeDll = Join-Path $PluginDir "StrandedDeepModSettings.dll"

if (-not (Test-Path -LiteralPath $BuildDll)) { throw "Build DLL missing. Run build.ps1 first: $BuildDll" }
New-Item -ItemType Directory -Force -Path $PluginDir | Out-Null
Copy-Item -LiteralPath $BuildDll -Destination $RuntimeDll -Force
$Pdb = [IO.Path]::ChangeExtension($BuildDll, ".pdb")
if (Test-Path -LiteralPath $Pdb) { Copy-Item -LiteralPath $Pdb -Destination (Join-Path $PluginDir "StrandedDeepModSettings.pdb") -Force }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "StrandedDeepModSettings.cs") -Destination (Join-Path $PluginDir "StrandedDeepModSettings.cs") -Force

$BuildHash = (Get-FileHash -LiteralPath $BuildDll -Algorithm SHA256).Hash
$RuntimeHash = (Get-FileHash -LiteralPath $RuntimeDll -Algorithm SHA256).Hash
Write-Host "BUILD SHA256  : $BuildHash"
Write-Host "RUNTIME SHA256: $RuntimeHash"
if ($BuildHash -ne $RuntimeHash) { throw "BUILD != RUNTIME for StrandedDeepModSettings" }
Write-Host "BUILD == RUNTIME: YES"
