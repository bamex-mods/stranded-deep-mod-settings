Stranded Deep Mod Settings — canonical host workspace

Plugin GUID: com.bamex.strandeddeep.modsettings
Version: 0.2.1
Known-good: 0.2.1 (native MODS page + selection auto-scroll field-tested)

Ownership:
- StrandedDeepModSettings.cs = host/UI/registry.
- SDK/API.md and SDK/ModSettingsClient.cs = authoritative shared SDK.
- UI Scaler source does NOT belong in this workspace.

Build:
  powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
Deploy (backup should be made by canonicalization orchestrator first):
  powershell -NoProfile -ExecutionPolicy Bypass -File .\deploy.ps1

Runtime:
  F:\SteamLibrary\steamapps\common\Stranded Deep\BepInEx\plugins\StrandedDeepModSettings\StrandedDeepModSettings.dll
