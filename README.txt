Stranded Deep Mod Settings — canonical host workspace

Plugin GUID: com.bamex.strandeddeep.modsettings
Version: 0.4.3
Status: FIELD-TESTED release source. Publication requires Distribution v1 verification (VERIFY OK / Runtime: MATCH).
Previous public stable: 0.2.1.

Ownership:
- StrandedDeepModSettings.cs = host/UI/registry/language-aware renderer.
- SDK/API.md and SDK/ModSettingsClient.cs = authoritative shared SDK.
- Client mods own their ConfigEntry/state/persistence/runtime behavior.
- UI Scaler source does NOT belong in this workspace.

v0.4.3 release highlights:
- Three-column MODS interface: native Options categories / registered mods / settings.
- ALL MODS shows the complete registry; selecting a mod filters the right renderer by modId.
- Existing client DLLs populate the selector through their existing RegisterMod calls; no rebuild is required.
- Middle selector and right settings use separate scrolling.
- Long mod names wrap in the middle column.
- Long setting names wrap in the right column while short rows remain compact.
- Slider/Toggle/Choice/Button control geometry remains unchanged horizontally.
- AddHeader / AddHeaderLocalized provide non-interactive visual subsections inside a mod.
- Header titles use the native category-header family with a smaller bold uppercase hierarchy and native separator.
- Automatic RU/EN localization follows the language already rendered by the native Stranded Deep Options UI.
- Old v0.2 single-language registrations remain supported.
- v0.3.1 native/MODS panel isolation and v0.2.1 settings auto-scroll are preserved.

Safety / compatibility:
- No game saves are modified.
- No client ConfigEntry ownership changes.
- No compile-time client reference to StrandedDeepModSettings.dll is required.
- Mod Settings remains a global system menu in local split-screen.

Build:
  powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -GameRoot "C:\Path\To\Stranded Deep"

Deploy:
  powershell -NoProfile -ExecutionPolicy Bypass -File .\deploy.ps1 -GameRoot "C:\Path\To\Stranded Deep"

Runtime:
  <GameRoot>\BepInEx\plugins\StrandedDeepModSettings\StrandedDeepModSettings.dll

GameRoot may also be supplied through STRANDED_DEEP_GAME_ROOT.
