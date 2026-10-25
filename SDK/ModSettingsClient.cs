using System;
using System.Reflection;
using BepInEx.Bootstrap;

namespace Bamex.StrandedDeep.ModSettings
{
    public static class ModSettingsClient
    {
        private const string HostGuid =
            "com.bamex.strandeddeep.modsettings";

        private const string ApiTypeName =
            "StrandedDeepModSettings.ModSettingsApi";

        public static bool IsAvailable()
        {
            return ResolveApiType() != null;
        }

        public static bool RegisterMod(
            string modId,
            string displayName,
            int order)
        {
            return Invoke(
                "RegisterMod",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(int)
                },
                new object[]
                {
                    modId,
                    displayName,
                    order
                });
        }

        public static bool AddSlider(
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
        {
            return Invoke(
                "AddSlider",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int),
                    typeof(float),
                    typeof(float),
                    typeof(float),
                    typeof(float),
                    typeof(string),
                    typeof(int),
                    typeof(Func<float>),
                    typeof(Action<float>)
                },
                new object[]
                {
                    modId,
                    settingId,
                    label,
                    order,
                    min,
                    max,
                    step,
                    displayMultiplier,
                    suffix,
                    decimals,
                    getter,
                    setter
                });
        }

        public static bool AddToggle(
            string modId,
            string settingId,
            string label,
            int order,
            string onText,
            string offText,
            Func<bool> getter,
            Action<bool> setter)
        {
            return Invoke(
                "AddToggle",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int),
                    typeof(string),
                    typeof(string),
                    typeof(Func<bool>),
                    typeof(Action<bool>)
                },
                new object[]
                {
                    modId,
                    settingId,
                    label,
                    order,
                    onText,
                    offText,
                    getter,
                    setter
                });
        }

        public static bool AddChoice(
            string modId,
            string settingId,
            string label,
            int order,
            string[] choices,
            Func<int> getter,
            Action<int> setter)
        {
            return Invoke(
                "AddChoice",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int),
                    typeof(string[]),
                    typeof(Func<int>),
                    typeof(Action<int>)
                },
                new object[]
                {
                    modId,
                    settingId,
                    label,
                    order,
                    choices,
                    getter,
                    setter
                });
        }

        public static bool AddButton(
            string modId,
            string settingId,
            string label,
            int order,
            string actionText,
            Action action)
        {
            return Invoke(
                "AddButton",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int),
                    typeof(string),
                    typeof(Action)
                },
                new object[]
                {
                    modId,
                    settingId,
                    label,
                    order,
                    actionText,
                    action
                });
        }

        public static bool RegisterModLocalized(
            string modId,
            string displayNameRussian,
            string displayNameEnglish,
            int order)
        {
            bool ok =
                Invoke(
                    "RegisterModLocalized",
                    new Type[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(int)
                    },
                    new object[]
                    {
                        modId,
                        displayNameRussian,
                        displayNameEnglish,
                        order
                    });

            if (ok)
                return true;

            return RegisterMod(
                modId,
                displayNameRussian,
                order);
        }

        public static bool AddSliderLocalized(
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
        {
            bool ok =
                Invoke(
                    "AddSliderLocalized",
                    new Type[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(int),
                        typeof(float),
                        typeof(float),
                        typeof(float),
                        typeof(float),
                        typeof(string),
                        typeof(string),
                        typeof(int),
                        typeof(Func<float>),
                        typeof(Action<float>)
                    },
                    new object[]
                    {
                        modId,
                        settingId,
                        labelRussian,
                        labelEnglish,
                        order,
                        min,
                        max,
                        step,
                        displayMultiplier,
                        suffixRussian,
                        suffixEnglish,
                        decimals,
                        getter,
                        setter
                    });

            if (ok)
                return true;

            return AddSlider(
                modId,
                settingId,
                labelRussian,
                order,
                min,
                max,
                step,
                displayMultiplier,
                suffixRussian,
                decimals,
                getter,
                setter);
        }

        public static bool AddToggleLocalized(
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
        {
            bool ok =
                Invoke(
                    "AddToggleLocalized",
                    new Type[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(int),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(Func<bool>),
                        typeof(Action<bool>)
                    },
                    new object[]
                    {
                        modId,
                        settingId,
                        labelRussian,
                        labelEnglish,
                        order,
                        onTextRussian,
                        onTextEnglish,
                        offTextRussian,
                        offTextEnglish,
                        getter,
                        setter
                    });

            if (ok)
                return true;

            return AddToggle(
                modId,
                settingId,
                labelRussian,
                order,
                onTextRussian,
                offTextRussian,
                getter,
                setter);
        }

        public static bool AddChoiceLocalized(
            string modId,
            string settingId,
            string labelRussian,
            string labelEnglish,
            int order,
            string[] choicesRussian,
            string[] choicesEnglish,
            Func<int> getter,
            Action<int> setter)
        {
            if (
                choicesRussian != null &&
                choicesEnglish != null &&
                choicesRussian.Length > 0 &&
                choicesEnglish.Length > 0 &&
                choicesRussian.Length != choicesEnglish.Length
            )
            {
                return false;
            }

            bool ok =
                Invoke(
                    "AddChoiceLocalized",
                    new Type[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(int),
                        typeof(string[]),
                        typeof(string[]),
                        typeof(Func<int>),
                        typeof(Action<int>)
                    },
                    new object[]
                    {
                        modId,
                        settingId,
                        labelRussian,
                        labelEnglish,
                        order,
                        choicesRussian,
                        choicesEnglish,
                        getter,
                        setter
                    });

            if (ok)
                return true;

            return AddChoice(
                modId,
                settingId,
                labelRussian,
                order,
                choicesRussian,
                getter,
                setter);
        }

        public static bool AddButtonLocalized(
            string modId,
            string settingId,
            string labelRussian,
            string labelEnglish,
            int order,
            string actionTextRussian,
            string actionTextEnglish,
            Action action)
        {
            bool ok =
                Invoke(
                    "AddButtonLocalized",
                    new Type[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(int),
                        typeof(string),
                        typeof(string),
                        typeof(Action)
                    },
                    new object[]
                    {
                        modId,
                        settingId,
                        labelRussian,
                        labelEnglish,
                        order,
                        actionTextRussian,
                        actionTextEnglish,
                        action
                    });

            if (ok)
                return true;

            return AddButton(
                modId,
                settingId,
                labelRussian,
                order,
                actionTextRussian,
                action);
        }

        public static bool SupportsHeaders()
        {
            return HasMethod(
                "AddHeader",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int)
                });
        }

        public static bool AddHeader(
            string modId,
            string headerId,
            string title,
            int order)
        {
            return Invoke(
                "AddHeader",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int)
                },
                new object[]
                {
                    modId,
                    headerId,
                    title,
                    order
                });
        }

        public static bool AddHeaderLocalized(
            string modId,
            string headerId,
            string titleRussian,
            string titleEnglish,
            int order)
        {
            bool ok =
                Invoke(
                    "AddHeaderLocalized",
                    new Type[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(int)
                    },
                    new object[]
                    {
                        modId,
                        headerId,
                        titleRussian,
                        titleEnglish,
                        order
                    });

            if (ok)
                return true;

            return AddHeader(
                modId,
                headerId,
                titleRussian,
                order);
        }

        // v0.5 API: assembly-neutral delegates, no host DLL dependency.
        public static bool SupportsFolderPicker()
        {
            return HasMethod("AddFolderPicker", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(int),
                typeof(string), typeof(Func<string>), typeof(Action<string>) });
        }

        public static bool AddFolderPicker(
            string modId, string settingId, string label, int order,
            string browseText, Func<string> getter, Action<string> setter)
        {
            return Invoke("AddFolderPicker", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(int),
                typeof(string), typeof(Func<string>), typeof(Action<string>) },
                new object[] { modId, settingId, label, order,
                    browseText, getter, setter });
        }

        public static bool AddFolderPickerLocalized(
            string modId, string settingId,
            string labelRussian, string labelEnglish, int order,
            string browseTextRussian, string browseTextEnglish,
            Func<string> getter, Action<string> setter)
        {
            return Invoke("AddFolderPickerLocalized", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(int), typeof(string), typeof(string),
                typeof(Func<string>), typeof(Action<string>) },
                new object[] { modId, settingId, labelRussian, labelEnglish,
                    order, browseTextRussian, browseTextEnglish, getter, setter });
        }

        public static bool AddStatus(
            string modId, string settingId, string label, int order,
            Func<string> textGetter, Func<int> severityGetter)
        {
            return Invoke("AddStatus", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(int),
                typeof(Func<string>), typeof(Func<int>) },
                new object[] { modId, settingId, label, order,
                    textGetter, severityGetter });
        }

        public static bool AddStatusLocalized(
            string modId, string settingId,
            string labelRussian, string labelEnglish, int order,
            Func<string> textGetter, Func<int> severityGetter)
        {
            return Invoke("AddStatusLocalized", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(int), typeof(Func<string>), typeof(Func<int>) },
                new object[] { modId, settingId, labelRussian, labelEnglish,
                    order, textGetter, severityGetter });
        }

        public static bool AddProgress(
            string modId, string settingId, string label, int order,
            Func<float> progressGetter, Func<string> statusTextGetter,
            Func<bool> indeterminateGetter, Func<bool> activeGetter)
        {
            return Invoke("AddProgress", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(int),
                typeof(Func<float>), typeof(Func<string>),
                typeof(Func<bool>), typeof(Func<bool>) },
                new object[] { modId, settingId, label, order, progressGetter,
                    statusTextGetter, indeterminateGetter, activeGetter });
        }

        public static bool AddProgressLocalized(
            string modId, string settingId,
            string labelRussian, string labelEnglish, int order,
            Func<float> progressGetter, Func<string> statusTextGetter,
            Func<bool> indeterminateGetter, Func<bool> activeGetter)
        {
            return Invoke("AddProgressLocalized", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(int), typeof(Func<float>), typeof(Func<string>),
                typeof(Func<bool>), typeof(Func<bool>) },
                new object[] { modId, settingId, labelRussian, labelEnglish,
                    order, progressGetter, statusTextGetter,
                    indeterminateGetter, activeGetter });
        }

        public static bool AddDynamicButton(
            string modId, string settingId, string label, int order,
            string actionText, Action action, Func<bool> isEnabled)
        {
            return Invoke("AddDynamicButton", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(int),
                typeof(string), typeof(Action), typeof(Func<bool>) },
                new object[] { modId, settingId, label, order,
                    actionText, action, isEnabled });
        }

        public static bool AddDynamicButtonLocalized(
            string modId, string settingId,
            string labelRussian, string labelEnglish, int order,
            string actionTextRussian, string actionTextEnglish,
            Action action, Func<bool> isEnabled)
        {
            return Invoke("AddDynamicButtonLocalized", new Type[] {
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(int), typeof(string), typeof(string),
                typeof(Action), typeof(Func<bool>) },
                new object[] { modId, settingId, labelRussian, labelEnglish,
                    order, actionTextRussian, actionTextEnglish, action, isEnabled });
        }

        public static bool RemoveMod(
            string modId)
        {
            return Invoke(
                "RemoveMod",
                new Type[]
                {
                    typeof(string)
                },
                new object[]
                {
                    modId
                });
        }

        private static bool HasMethod(
            string methodName,
            Type[] parameterTypes)
        {
            try
            {
                Type apiType =
                    ResolveApiType();

                if (apiType == null)
                    return false;

                MethodInfo method =
                    apiType.GetMethod(
                        methodName,
                        BindingFlags.Public |
                        BindingFlags.Static,
                        null,
                        parameterTypes,
                        null);

                return method != null;
            }
            catch
            {
                return false;
            }
        }

        private static bool Invoke(
            string methodName,
            Type[] parameterTypes,
            object[] arguments)
        {
            try
            {
                Type apiType =
                    ResolveApiType();

                if (apiType == null)
                    return false;

                MethodInfo method =
                    apiType.GetMethod(
                        methodName,
                        BindingFlags.Public |
                        BindingFlags.Static,
                        null,
                        parameterTypes,
                        null);

                if (method == null)
                    return false;

                method.Invoke(
                    null,
                    arguments);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Type ResolveApiType()
        {
            try
            {
                if (
                    !Chainloader.PluginInfos.ContainsKey(
                        HostGuid)
                )
                {
                    return null;
                }

                BepInEx.BaseUnityPlugin plugin =
                    Chainloader
                        .PluginInfos[HostGuid]
                        .Instance;

                if (plugin == null)
                    return null;

                Assembly assembly =
                    plugin
                        .GetType()
                        .Assembly;

                return assembly.GetType(
                    ApiTypeName,
                    false);
            }
            catch
            {
                return null;
            }
        }
    }
}
