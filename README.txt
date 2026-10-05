Stranded Deep Mod Settings — canonical host workspace

Plugin GUID: com.bamex.strandeddeep.modsettings
Version: 0.3.1
Status: known-good; RU/EN localization, auto-scroll and panel isolation field-tested.
Previous public version: 0.2.1; version 0.3.0 was not promoted because of the panel overlap regression fixed in 0.3.1.

Ownership:
- StrandedDeepModSettings.cs = host/UI/registry/language-aware renderer.
- SDK/API.md and SDK/ModSettingsClient.cs = authoritative shared SDK.
- Client mods own their ConfigEntry/state/persistence/runtime behavior.
- UI Scaler source does NOT belong in this workspace.

Localization:
- Host follows the language already rendered by native Stranded Deep Options UI.
- Russian UI -> МОДЫ and RU strings from localized registrations.
- English/non-Russian UI -> MODS and EN strings from localized registrations.
- Old single-language v0.2 API remains compatible.

Build:
  powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -GameRoot "C:\Path\To\Stranded Deep"

Deploy:
  powershell -NoProfile -ExecutionPolicy Bypass -File .\deploy.ps1 -GameRoot "C:\Path\To\Stranded Deep"

Runtime:
  <GameRoot>\BepInEx\plugins\StrandedDeepModSettings\StrandedDeepModSettings.dll

GameRoot may also be supplied through STRANDED_DEEP_GAME_ROOT.

Panel isolation fix:
- If any native Options panel is active, Options - Mods is forced inactive.
- Prevents MODS categories from appearing below GENERAL in the shared ScrollRect.
- No API or SDK changes.
- UI Scaler is not modified.
