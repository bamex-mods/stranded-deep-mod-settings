# Stranded Deep Mod Settings API v0.5

This is the shared native-style Options UI host for Stranded Deep BepInEx mods.

Core rule
---------

The Mod Settings plugin owns UI only.
Each client mod owns its own ConfigEntry/state, validation, persistence and runtime behavior.

Client mods do not compile against StrandedDeepModSettings.dll.
They vendor SDK\ModSettingsClient.cs and use a SoftDependency. The helper discovers the host at runtime through BepInEx Chainloader + reflection.

If StrandedDeepModSettings is absent, the client mod must still load and work; only Settings -> MODS integration is unavailable.

v0.4 UI model
-------------

Settings -> MODS is rendered as three columns:

1. Native Stranded Deep Options categories.
2. Mod selector: ALL MODS plus every mod registered through RegisterMod/RegisterModLocalized.
3. Settings content.

Selecting ALL MODS renders the full registry, matching the previous v0.3 behavior.
Selecting a specific mod renders only entries whose modId matches that mod.

The mod selector is a host-side feature. Existing client DLLs require no source change or rebuild to appear in it.

Language model
--------------

The host follows the language already rendered by the native Stranded Deep Options UI.

Russian game UI -> Russian registered strings.
English/non-Russian game UI -> English registered strings.

The host does not add a separate language setting.

The v0.2 single-language API remains available and binary/source compatible. Existing client mods continue to work unchanged, but their single registered string is shown in both game languages. To get automatic RU/EN switching, use the *Localized methods.

Public compatibility API (v0.2)
-------------------------------

RegisterMod(
    string modId,
    string displayName,
    int order)

AddSlider(
    string modId,
    string settingId,
    string label,
    int order,
    float min,
    float max,
    float step,
    float displayMultiplier,
    string suffix,
    int decimals,
    Func<float> getter,
    Action<float> setter)

AddToggle(
    string modId,
    string settingId,
    string label,
    int order,
    string onText,
    string offText,
    Func<bool> getter,
    Action<bool> setter)

AddChoice(
    string modId,
    string settingId,
    string label,
    int order,
    string[] choices,
    Func<int> getter,
    Action<int> setter)

AddButton(
    string modId,
    string settingId,
    string label,
    int order,
    string actionText,
    Action action)

RemoveMod(string modId)

Bilingual API (v0.3)
--------------------

RegisterModLocalized(
    string modId,
    string displayNameRussian,
    string displayNameEnglish,
    int order)

AddSliderLocalized(
    string modId,
    string settingId,
    string labelRussian,
    string labelEnglish,
    int order,
    float min,
    float max,
    float step,
    float displayMultiplier,
    string suffixRussian,
    string suffixEnglish,
    int decimals,
    Func<float> getter,
    Action<float> setter)

AddToggleLocalized(
    string modId,
    string settingId,
    string labelRussian,
    string labelEnglish,
    int order,
    string onTextRussian,
    string onTextEnglish,
    string offTextRussian,
    string offTextEnglish,
    Func<bool> getter,
    Action<bool> setter)

AddChoiceLocalized(
    string modId,
    string settingId,
    string labelRussian,
    string labelEnglish,
    int order,
    string[] choicesRussian,
    string[] choicesEnglish,
    Func<int> getter,
    Action<int> setter)

AddButtonLocalized(
    string modId,
    string settingId,
    string labelRussian,
    string labelEnglish,
    int order,
    string actionTextRussian,
    string actionTextEnglish,
    Action action)

Section Header API (v0.4)
-------------------------

AddHeader(
    string modId,
    string headerId,
    string title,
    int order)

AddHeaderLocalized(
    string modId,
    string headerId,
    string titleRussian,
    string titleEnglish,
    int order)

Header is visual only:

- it has no ConfigEntry;
- it has no getter/setter/action;
- it is sorted together with controls by order;
- it is not selectable;
- controller navigation skips it;
- repeated registration of the same headerId updates that header rather than duplicating it;
- RemoveMod removes its headers together with its controls.

Header IDs are independent from normal setting IDs. A header and a normal setting may use the same text ID without replacing each other, although using distinct IDs is recommended for clarity.

Client capability check
-----------------------

The v0.4 SDK adds:

SupportsHeaders()

Use this when a client wants a graceful visual fallback on an older host.

Example:

    if (ModSettingsClient.SupportsHeaders())
    {
        ModSettingsClient.AddHeaderLocalized(
            "rarefinds",
            "vanilla_loot",
            "ОБЫЧНЫЙ ЛУТ",
            "VANILLA LOOT",
            100);

        ModSettingsClient.AddSliderLocalized(
            "rarefinds",
            "vehicle_parts",
            "Детали транспорта",
            "Vehicle Parts",
            110,
            0f,
            1f,
            0.05f,
            100f,
            "%",
            "%",
            0,
            GetVehicleParts,
            SetVehicleParts);
    }
    else
    {
        // Optional compatibility presentation for an older host.
        ModSettingsClient.AddSlider(
            "rarefinds",
            "vehicle_parts",
            "Обычный лут: Детали транспорта",
            110,
            0f,
            1f,
            0.05f,
            100f,
            "%",
            0,
            GetVehicleParts,
            SetVehicleParts);
    }

If AddHeader/AddHeaderLocalized is called against an older host, the helper returns false and does not throw into the client mod.

Backward compatibility
----------------------

The following must remain true:

- old v0.2/v0.3 client DLLs work with the v0.4 host without recompilation;
- old method signatures are unchanged;
- no client gets a compile-time reference to StrandedDeepModSettings.dll;
- Mod Settings never becomes the owner of a client's configuration values;
- the middle mod-selector column is generated entirely from existing RegisterMod registrations;
- Header is opt-in and is not required for existing mods.

Rendering and navigation
------------------------

The host reuses the native Stranded Deep Options Canvas, TMP fonts, option controls, category style and ScrollRect behavior.

The v0.4 MODS view keeps the native left Options categories, adds a host-owned middle mod list, and retains the existing right settings renderer.

ALL MODS is always the first selector entry.
The middle selector scrolls independently from the right settings list.
The right list keeps the v0.2.1 auto-scroll-on-selection behavior.

When a specific mod is selected, only that mod's registered entries are rendered on the right. ALL MODS renders all registered mods in normal mod/order sorting.

Header entries are layout-only and never receive focus.

v0.5.0 API additions (field-tested 2026-10-25)
--------------------------------------------------

New host methods preserve every v0.4.3 public API signature:

    AddFolderPicker(string modId, string settingId, string label, int order,
        string browseText, Func<string> getter, Action<string> setter)
    AddFolderPickerLocalized(string modId, string settingId,
        string labelRussian, string labelEnglish, int order,
        string browseTextRussian, string browseTextEnglish,
        Func<string> getter, Action<string> setter)

    AddStatus(string modId, string settingId, string label, int order,
        Func<string> textGetter, Func<int> severityGetter)
    AddStatusLocalized(string modId, string settingId,
        string labelRussian, string labelEnglish, int order,
        Func<string> textGetter, Func<int> severityGetter)

    AddProgress(string modId, string settingId, string label, int order,
        Func<float> progressGetter, Func<string> statusTextGetter,
        Func<bool> indeterminateGetter, Func<bool> activeGetter)
    AddProgressLocalized(string modId, string settingId,
        string labelRussian, string labelEnglish, int order,
        Func<float> progressGetter, Func<string> statusTextGetter,
        Func<bool> indeterminateGetter, Func<bool> activeGetter)

    AddDynamicButton(string modId, string settingId, string label, int order,
        string actionText, Action action, Func<bool> isEnabled)
    AddDynamicButtonLocalized(string modId, string settingId,
        string labelRussian, string labelEnglish, int order,
        string actionTextRussian, string actionTextEnglish,
        Action action, Func<bool> isEnabled)

Status severity: 0 Info, 1 Success, 2 Warning, 3 Error.
Progress range: 0.0 to 1.0. activeGetter=false hides the row.
Folder picker calls setter only when a folder is confirmed. Host never persists paths.
Getters must be cheap, side-effect-free, and safe on Unity main thread.
SDK supports new methods by reflection; if host is absent or old, wrapper returns false.


Release compatibility note
--------------------------

The v0.5.0 host is an additive extension of v0.4.3. Existing public methods
retain their signatures. New read-only Status/Progress entries do not receive
controller focus, and values update through the established runtime refresh.

The host owns UI only. Clients own the data, validation, and sidecars.
