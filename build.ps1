param(
    [string]$GameRoot = $env:STRANDED_DEEP_GAME_ROOT
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($GameRoot)) {
    throw "GameRoot was not supplied. Pass -GameRoot or set STRANDED_DEEP_GAME_ROOT."
}
$Managed = Join-Path $GameRoot "Stranded_Deep_Data\Managed"
$BepInExCore = Join-Path $GameRoot "BepInEx\core"
$Compiler = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

$Source = Join-Path $PSScriptRoot "StrandedDeepModSettings.cs"
$OutDir = Join-Path $PSScriptRoot "out"
$Output = Join-Path $OutDir "StrandedDeepModSettings.dll"

foreach ($Required in @(
    $Compiler,
    $Source,
    (Join-Path $BepInExCore "BepInEx.dll"),
    (Join-Path $Managed "Assembly-CSharp.dll"),
    (Join-Path $Managed "Unity.TextMeshPro.dll")
)) {
    if (-not (Test-Path -LiteralPath $Required)) {
        throw "Required file not found: $Required"
    }
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
Remove-Item -LiteralPath $Output -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath ([IO.Path]::ChangeExtension($Output, ".pdb")) -Force -ErrorAction SilentlyContinue

$Refs = New-Object System.Collections.Generic.List[string]
$Refs.Add((Join-Path $BepInExCore "BepInEx.dll"))
Get-ChildItem -LiteralPath $Managed -Filter "UnityEngine*.dll" -File |
    Sort-Object Name |
    ForEach-Object { $Refs.Add($_.FullName) }
$Refs.Add((Join-Path $Managed "Assembly-CSharp.dll"))
$Refs.Add((Join-Path $Managed "Unity.TextMeshPro.dll"))

$Args = New-Object System.Collections.Generic.List[string]
$Args.Add("/nologo")
$Args.Add("/target:library")
$Args.Add("/optimize+")
$Args.Add("/debug:pdbonly")
$Args.Add("/langversion:5")
$Args.Add("/out:$Output")
foreach ($Ref in $Refs) { $Args.Add("/reference:$Ref") }
$Args.Add($Source)

Write-Host "=== BUILD StrandedDeepModSettings v0.3.1 ==="
Write-Host "Workspace: $PSScriptRoot"
Write-Host "References: $($Refs.Count)"
& $Compiler $Args.ToArray()
if ($LASTEXITCODE -ne 0) { throw "Compilation failed: exit $LASTEXITCODE" }
if (-not (Test-Path -LiteralPath $Output)) { throw "Output missing: $Output" }

$Hash = (Get-FileHash -LiteralPath $Output -Algorithm SHA256).Hash
Write-Host "=== BUILD OK ==="
Write-Host "DLL: $Output"
Write-Host "SHA256: $Hash"
