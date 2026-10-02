$ErrorActionPreference = "Stop"
$BackupsRoot = "F:\mod-work\backups"
$GameRoot = "F:\SteamLibrary\steamapps\common\Stranded Deep"
$PluginDir = Join-Path $GameRoot "BepInEx\plugins\StrandedDeepModSettings"
$ConfigDir = Join-Path $GameRoot "BepInEx\config"
$ConfigName = "com.bamex.strandeddeep.modsettings.cfg"

$Backup = Get-ChildItem -LiteralPath $BackupsRoot -Directory -Filter "Canonical-ModSettings-UiScaler-before-*" |
    Sort-Object Name -Descending | Select-Object -First 1
if ($null -eq $Backup) { throw "No canonicalization backup found." }
$SourcePlugin = Join-Path $Backup.FullName "runtime\StrandedDeepModSettings"
if (-not (Test-Path -LiteralPath $SourcePlugin)) { throw "Host plugin backup not found: $SourcePlugin" }
Remove-Item -LiteralPath $PluginDir -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item -LiteralPath $SourcePlugin -Destination $PluginDir -Recurse -Force
$Cfg = Join-Path $Backup.FullName ("config\" + $ConfigName)
if (Test-Path -LiteralPath $Cfg) { New-Item -ItemType Directory -Force -Path $ConfigDir | Out-Null; Copy-Item -LiteralPath $Cfg -Destination (Join-Path $ConfigDir $ConfigName) -Force }
Write-Host "Restored StrandedDeepModSettings from: $($Backup.FullName)"
