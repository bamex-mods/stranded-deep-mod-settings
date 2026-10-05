# Stranded Deep Mod Settings API v0.3

This is the shared native-style Options UI host for our Stranded Deep BepInEx mods.

Core rule
---------

The Mod Settings plugin owns UI only.
Each client mod owns its own ConfigEntry/state, validation, persistence and runtime behavior.

Client mods do not compile against StrandedDeepModSettings.dll.
They vendor SDK\ModSettingsClient.cs and use a SoftDependency. The helper discovers the host at runtime through BepInEx Chainloader + reflection.

If StrandedDeepModSettings is absent, the client mod must still load and work; only the Settings -> MODS integration is unavailable.

Language model in v0.3
----------------------

The host follows the language already rendered by the native Stranded Deep Options UI.

For the supported bilingual API:

Russian game UI -> Russian registered strings
English/non-Russian game UI -> English registered strings

The host does not add a separate language setting.

The v0.2 single-language API remains available and binary/source compatible. Existing client mods continue to work unchanged, but their single registered string is shown in both game languages. To get automatic RU/EN switching, migrate the client to the *Localized methods below.

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

The RU and EN choice arrays must contain the same number of elements.

AddButtonLocalized(
    string modId,
    string settingId,
    string labelRussian,
    string labelEnglish,
    int order,
    string actionTextRussian,
    string actionTextEnglish,
    Action action)

Client integration
------------------

Copy the authoritative helper:

SDK\ModSettingsClient.cs

into the client mod source tree.

Recommended BepInEx dependency:

[BepInDependency(
    "com.bamex.strandeddeep.modsettings",
    BepInDependency.DependencyFlags.SoftDependency)]

Example bilingual slider
------------------------

ModSettingsClient.RegisterModLocalized(
    "example",
    "Интерфейс",
    "Interface",
    500);

ModSettingsClient.AddSliderLocalized(
    "example",
    "scale",
    "Масштаб интерфейса",
    "UI Scale",
    100,
    1.0f,
    2.0f,
    0.1f,
    100.0f,
    "%",
    "%",
    0,
    GetScale,
    SetScale);

Example bilingual toggle
------------------------

ModSettingsClient.AddToggleLocalized(
    "example",
    "enabled",
    "Включено",
    "Enabled",
    200,
    "Вкл.",
    "On",
    "Выкл.",
    "Off",
    GetEnabled,
    SetEnabled);

Example bilingual choice
------------------------

string[] choicesRu = new string[]
{
    "Низко",
    "Средне",
    "Высоко"
};

string[] choicesEn = new string[]
{
    "Low",
    "Medium",
    "High"
};

ModSettingsClient.AddChoiceLocalized(
    "example",
    "quality",
    "Качество",
    "Quality",
    300,
    choicesRu,
    choicesEn,
    GetQualityIndex,
    SetQualityIndex);

Example bilingual action
------------------------

ModSettingsClient.AddButtonLocalized(
    "example",
    "reset",
    "Сбросить настройки",
    "Reset settings",
    900,
    "Сбросить",
    "Reset",
    ResetSettings);

Backward compatibility behavior
-------------------------------

The v0.3 ModSettingsClient helper tries the new localized host method first.
If it is running with an older v0.2 host that does not expose that method, the helper falls back to the old single-language call using the Russian text. This preserves soft dependency and allows client mods to remain functional during staged upgrades.

Rendering model
---------------

Each registered mod becomes a native category/header inside Settings -> MODS.
The page uses the game's existing Options Canvas, ScrollRect, TMP fonts, category header, slider/button templates, animations and layout.

The host watches RegistryVersion and rebuilds the page when registrations change. Since v0.2.1, selected controls also auto-scroll into view for controller/keyboard navigation.
