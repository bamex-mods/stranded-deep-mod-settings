Stranded Deep Mod Settings v0.5.0
=================================

GUID: com.bamex.strandeddeep.modsettings
Host: StrandedDeepModSettings.ModSettingsApi
Source repository: https://github.com/bamex-mods/stranded-deep-mod-settings

FEATURES
--------
Native-style Settings -> MODS: 3 columns (game tabs / mods / controls),
localization RU/EN, independent list scrolling, long-name wrapping,
headers and legacy Slider/Toggle/Choice/Button.

New in v0.5.0:
- FolderPicker: native Windows system folder selection via IFileDialog on STA thread;
  only confirmed selections call client setter on Unity main thread.
- Status: read-only dynamic status text, levels Info/Success/Warning/Error.
- Progress: read-only progress with determinate/indeterminate modes and visibility.
- DynamicButton: clickable action whose enabled state is client-controlled.
- Localized variants and reflection-based SDK wrappers for all new types.

ARCHITECTURE
------------
Client mod owns config, game logic, persistence, and validation.
Mod Settings owns registry, UI and delegate invocation only.
SDK/ModSettingsClient.cs is compiled into client mods and uses BepInEx soft
runtime reflection, without compile-time host dependency.

COMPATIBILITY
-------------
Existing client method signatures are unchanged. Older mods do not need
recompiling. If the host is absent, client gameplay logic remains operational.
This host is Windows-focused because native FolderPicker requires Windows Shell.

FIELD TEST
----------
User confirmed successful candidate r1 compilation, test deployment,
and in-game operation on 2026-10-25, local split-screen Windows setup.
Tested runtime DLL SHA-256:
38DD49305050CE877067ECB8F05F87257BD7EB5C16E601C12726BA35BEA097A2
This is the only runtime binary approved for v0.5.0 release packaging.
The host source is unchanged from the field-tested r1 candidate.

BUILD
-----
  powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -GameRoot $env:STRANDED_DEEP_GAME_ROOT

WARNING: building from canonical source may generate a distinct DLL byte hash
and does NOT mean it replaces the field-tested runtime. Do not deploy a
new build over the proven runtime without a new in-game test.

DISTRIBUTION
------------
Source of truth: dist/distribution.json
Release packages are generated from tested runtime by the shared
the shared distribution-v1 packaging and verification tools.
No test plugin is part of the public runtime package.
