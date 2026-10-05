using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StrandedDeepModSettings
{
    public enum ModSettingKind
    {
        Slider = 0,
        Toggle = 1,
        Choice = 2,
        Button = 3
    }

    internal class ModSettingDefinition
    {
        public string Id;
        public string LabelRussian;
        public string LabelEnglish;
        public int Order;
        public ModSettingKind Kind;

        public float Min;
        public float Max;
        public float Step;
        public float DisplayMultiplier;
        public string SuffixRussian;
        public string SuffixEnglish;
        public int Decimals;
        public Func<float> FloatGetter;
        public Action<float> FloatSetter;

        public string OnTextRussian;
        public string OnTextEnglish;
        public string OffTextRussian;
        public string OffTextEnglish;
        public Func<bool> BoolGetter;
        public Action<bool> BoolSetter;

        public string[] ChoicesRussian;
        public string[] ChoicesEnglish;
        public Func<int> ChoiceGetter;
        public Action<int> ChoiceSetter;

        public string ActionTextRussian;
        public string ActionTextEnglish;
        public Action ButtonAction;
    }

    internal class ModDefinition
    {
        public string Id;
        public string DisplayNameRussian;
        public string DisplayNameEnglish;
        public int Order;
        public readonly List<ModSettingDefinition> Settings =
            new List<ModSettingDefinition>();
    }

    public static class ModSettingsApi
    {
        private static readonly Dictionary<string, ModDefinition> _mods =
            new Dictionary<string, ModDefinition>(
                StringComparer.OrdinalIgnoreCase);

        private static int _registryVersion;

        public static int RegistryVersion
        {
            get { return _registryVersion; }
        }

        // v0.2 compatibility API. A single supplied text is used for both
        // languages so existing client mods continue to work unchanged.
        public static void RegisterMod(
            string modId,
            string displayName,
            int order)
        {
            RegisterModLocalized(
                modId,
                displayName,
                displayName,
                order);
        }

        // v0.3 bilingual API. The host follows the language already rendered
        // by the native Stranded Deep Options UI (RU/EN).
        public static void RegisterModLocalized(
            string modId,
            string displayNameRussian,
            string displayNameEnglish,
            int order)
        {
            if (String.IsNullOrEmpty(modId))
                throw new ArgumentException("modId");

            ModDefinition mod;

            if (!_mods.TryGetValue(modId, out mod))
            {
                mod = new ModDefinition();
                mod.Id = modId;
                _mods.Add(modId, mod);
            }

            mod.DisplayNameRussian =
                FirstNonEmpty(
                    displayNameRussian,
                    displayNameEnglish,
                    modId);

            mod.DisplayNameEnglish =
                FirstNonEmpty(
                    displayNameEnglish,
                    displayNameRussian,
                    modId);

            mod.Order = order;
            Touch();
        }

        public static void AddSlider(
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
            AddSliderLocalized(
                modId,
                settingId,
                label,
                label,
                order,
                min,
                max,
                step,
                displayMultiplier,
                suffix,
                suffix,
                decimals,
                getter,
                setter);
        }

        public static void AddSliderLocalized(
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
            ModSettingDefinition setting =
                NewSetting(
                    modId,
                    settingId,
                    labelRussian,
                    labelEnglish,
                    order,
                    ModSettingKind.Slider);

            setting.Min = min;
            setting.Max = max;
            setting.Step = step <= 0f ? 0.01f : step;
            setting.DisplayMultiplier =
                displayMultiplier == 0f
                ? 1f
                : displayMultiplier;

            setting.SuffixRussian =
                suffixRussian ?? "";

            setting.SuffixEnglish =
                suffixEnglish ?? "";

            setting.Decimals =
                decimals < 0 ? 0 : decimals;

            setting.FloatGetter = getter;
            setting.FloatSetter = setter;

            UpsertSetting(
                modId,
                setting);
        }

        public static void AddToggle(
            string modId,
            string settingId,
            string label,
            int order,
            string onText,
            string offText,
            Func<bool> getter,
            Action<bool> setter)
        {
            string normalizedOn =
                String.IsNullOrEmpty(onText)
                ? "Вкл."
                : onText;

            string normalizedOff =
                String.IsNullOrEmpty(offText)
                ? "Выкл."
                : offText;

            AddToggleLocalized(
                modId,
                settingId,
                label,
                label,
                order,
                normalizedOn,
                normalizedOn,
                normalizedOff,
                normalizedOff,
                getter,
                setter);
        }

        public static void AddToggleLocalized(
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
            ModSettingDefinition setting =
                NewSetting(
                    modId,
                    settingId,
                    labelRussian,
                    labelEnglish,
                    order,
                    ModSettingKind.Toggle);

            setting.OnTextRussian =
                FirstNonEmpty(
                    onTextRussian,
                    onTextEnglish,
                    "Вкл.");

            setting.OnTextEnglish =
                FirstNonEmpty(
                    onTextEnglish,
                    onTextRussian,
                    "On");

            setting.OffTextRussian =
                FirstNonEmpty(
                    offTextRussian,
                    offTextEnglish,
                    "Выкл.");

            setting.OffTextEnglish =
                FirstNonEmpty(
                    offTextEnglish,
                    offTextRussian,
                    "Off");

            setting.BoolGetter = getter;
            setting.BoolSetter = setter;

            UpsertSetting(
                modId,
                setting);
        }

        public static void AddChoice(
            string modId,
            string settingId,
            string label,
            int order,
            string[] choices,
            Func<int> getter,
            Action<int> setter)
        {
            AddChoiceLocalized(
                modId,
                settingId,
                label,
                label,
                order,
                choices,
                choices,
                getter,
                setter);
        }

        public static void AddChoiceLocalized(
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
            string[] ru =
                choicesRussian ?? new string[0];

            string[] en =
                choicesEnglish ?? new string[0];

            if (ru.Length == 0 && en.Length > 0)
                ru = en;

            if (en.Length == 0 && ru.Length > 0)
                en = ru;

            if (ru.Length != en.Length)
            {
                throw new ArgumentException(
                    "Localized choice arrays must have the same length.");
            }

            ModSettingDefinition setting =
                NewSetting(
                    modId,
                    settingId,
                    labelRussian,
                    labelEnglish,
                    order,
                    ModSettingKind.Choice);

            setting.ChoicesRussian = ru;
            setting.ChoicesEnglish = en;
            setting.ChoiceGetter = getter;
            setting.ChoiceSetter = setter;

            UpsertSetting(
                modId,
                setting);
        }

        public static void AddButton(
            string modId,
            string settingId,
            string label,
            int order,
            string actionText,
            Action action)
        {
            string normalizedAction =
                String.IsNullOrEmpty(actionText)
                ? "Выполнить"
                : actionText;

            AddButtonLocalized(
                modId,
                settingId,
                label,
                label,
                order,
                normalizedAction,
                normalizedAction,
                action);
        }

        public static void AddButtonLocalized(
            string modId,
            string settingId,
            string labelRussian,
            string labelEnglish,
            int order,
            string actionTextRussian,
            string actionTextEnglish,
            Action action)
        {
            ModSettingDefinition setting =
                NewSetting(
                    modId,
                    settingId,
                    labelRussian,
                    labelEnglish,
                    order,
                    ModSettingKind.Button);

            setting.ActionTextRussian =
                FirstNonEmpty(
                    actionTextRussian,
                    actionTextEnglish,
                    "Выполнить");

            setting.ActionTextEnglish =
                FirstNonEmpty(
                    actionTextEnglish,
                    actionTextRussian,
                    "Run");

            setting.ButtonAction = action;

            UpsertSetting(
                modId,
                setting);
        }

        public static void RemoveMod(
            string modId)
        {
            if (String.IsNullOrEmpty(modId))
                return;

            if (_mods.Remove(modId))
            {
                Touch();
            }
        }

        internal static List<ModDefinition> GetSnapshot()
        {
            List<ModDefinition> result =
                new List<ModDefinition>();

            foreach (
                KeyValuePair<string, ModDefinition> pair
                in _mods)
            {
                result.Add(pair.Value);
            }

            result.Sort(
                delegate(
                    ModDefinition a,
                    ModDefinition b)
                {
                    int order =
                        a.Order.CompareTo(b.Order);

                    if (order != 0)
                        return order;

                    return String.Compare(
                        a.Id,
                        b.Id,
                        StringComparison.OrdinalIgnoreCase);
                });

            foreach (ModDefinition mod in result)
            {
                mod.Settings.Sort(
                    delegate(
                        ModSettingDefinition a,
                        ModSettingDefinition b)
                    {
                        int order =
                            a.Order.CompareTo(b.Order);

                        if (order != 0)
                            return order;

                        return String.Compare(
                            a.Id,
                            b.Id,
                            StringComparison.OrdinalIgnoreCase);
                    });
            }

            return result;
        }

        private static ModSettingDefinition NewSetting(
            string modId,
            string settingId,
            string labelRussian,
            string labelEnglish,
            int order,
            ModSettingKind kind)
        {
            if (String.IsNullOrEmpty(modId))
                throw new ArgumentException("modId");

            if (String.IsNullOrEmpty(settingId))
                throw new ArgumentException("settingId");

            EnsureMod(modId);

            ModSettingDefinition setting =
                new ModSettingDefinition();

            setting.Id = settingId;
            setting.LabelRussian =
                FirstNonEmpty(
                    labelRussian,
                    labelEnglish,
                    settingId);

            setting.LabelEnglish =
                FirstNonEmpty(
                    labelEnglish,
                    labelRussian,
                    settingId);

            setting.Order = order;
            setting.Kind = kind;

            return setting;
        }

        private static string FirstNonEmpty(
            string primary,
            string secondary,
            string fallback)
        {
            if (!String.IsNullOrEmpty(primary))
                return primary;

            if (!String.IsNullOrEmpty(secondary))
                return secondary;

            return fallback ?? "";
        }

        private static void EnsureMod(
            string modId)
        {
            if (_mods.ContainsKey(modId))
                return;

            ModDefinition mod =
                new ModDefinition();

            mod.Id = modId;
            mod.DisplayNameRussian = modId;
            mod.DisplayNameEnglish = modId;
            mod.Order = 1000;

            _mods.Add(
                modId,
                mod);
        }

        private static void UpsertSetting(
            string modId,
            ModSettingDefinition setting)
        {
            ModDefinition mod =
                _mods[modId];

            for (
                int i = 0;
                i < mod.Settings.Count;
                i++)
            {
                if (
                    String.Equals(
                        mod.Settings[i].Id,
                        setting.Id,
                        StringComparison.OrdinalIgnoreCase)
                )
                {
                    mod.Settings[i] =
                        setting;

                    Touch();
                    return;
                }
            }

            mod.Settings.Add(setting);
            Touch();
        }

        private static void Touch()
        {
            _registryVersion++;

            if (_registryVersion < 0)
                _registryVersion = 1;
        }
    }

    internal class RuntimeControl
    {
        public ModSettingDefinition Definition;
        public Slider Slider;
        public Button Button;
        public TMP_Text ValueLabel;
        public TMP_Text NameLabel;
        public bool Suppress;
    }

    internal class ScrollIntoViewOnSelect : MonoBehaviour, ISelectHandler
    {
        private const float DefaultMargin = 24f;

        private ScrollRect _scrollRect;
        private RectTransform _target;

        public void Configure(
            ScrollRect scrollRect,
            RectTransform target)
        {
            _scrollRect = scrollRect;
            _target = target;
        }

        public void OnSelect(
            BaseEventData eventData)
        {
            EnsureVisible();
        }

        private void EnsureVisible()
        {
            if (
                _scrollRect == null ||
                _target == null ||
                _scrollRect.content == null
            )
            {
                return;
            }

            RectTransform viewport =
                _scrollRect.viewport;

            if (viewport == null)
            {
                viewport =
                    _scrollRect.GetComponent<RectTransform>();
            }

            if (viewport == null)
                return;

            Canvas.ForceUpdateCanvases();

            Bounds targetBounds =
                RectTransformUtility
                    .CalculateRelativeRectTransformBounds(
                        viewport,
                        _target);

            Rect viewportRect =
                viewport.rect;

            float deltaY = 0f;

            float bottomLimit =
                viewportRect.yMin +
                DefaultMargin;

            float topLimit =
                viewportRect.yMax -
                DefaultMargin;

            if (targetBounds.min.y < bottomLimit)
            {
                deltaY =
                    bottomLimit -
                    targetBounds.min.y;
            }
            else if (targetBounds.max.y > topLimit)
            {
                deltaY =
                    topLimit -
                    targetBounds.max.y;
            }

            if (Math.Abs(deltaY) < 0.01f)
                return;

            _scrollRect.StopMovement();

            Vector2 anchored =
                _scrollRect.content.anchoredPosition;

            anchored.y += deltaY;

            _scrollRect.content.anchoredPosition =
                anchored;

            _scrollRect.verticalNormalizedPosition =
                Mathf.Clamp01(
                    _scrollRect.verticalNormalizedPosition);

            Canvas.ForceUpdateCanvases();
        }
    }

    [BepInPlugin(
        "com.bamex.strandeddeep.modsettings",
        "Stranded Deep Mod Settings",
        "0.3.1")]
    public class ModSettingsPlugin : BaseUnityPlugin
    {
        private const float ScanInterval = 0.50f;

        private Transform _optionsRoot;
        private Transform _content;
        private ScrollRect _scrollRect;

        private Transform _generalPanel;
        private Transform _graphicsPanel;
        private Transform _audioPanel;
        private Transform _inputPanel;
        private Transform _aboutPanel;
        private Transform _generalButtonTransform;

        private Transform _sourceCategory;
        private Transform _sourceSlider;
        private Transform _sourceOptionButton;

        private readonly List<Transform> _nativePanels =
            new List<Transform>();

        private readonly List<RuntimeControl> _runtimeControls =
            new List<RuntimeControl>();

        private GameObject _modsPanel;
        private Button _modsButton;

        private float _nextScanTime;
        private bool _wasOptionsActive;
        private int _builtRegistryVersion = -1;
        private bool _languageInitialized;
        private bool _useRussian;
        private bool _panelOverlapLogged;

        private void Awake()
        {
            Logger.LogInfo(
                "Stranded Deep Mod Settings API v0.3.1 loaded.");

            Logger.LogInfo(
                "Waiting for the native Options menu.");
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextScanTime)
                return;

            _nextScanTime =
                Time.unscaledTime + ScanInterval;

            if (_optionsRoot == null)
            {
                Transform found =
                    FindOptionsRoot();

                if (found != null)
                {
                    TryInject(found);
                }

                return;
            }

            if (_optionsRoot == null)
                return;

            bool active =
                _optionsRoot.gameObject.activeInHierarchy;

            bool languageChanged =
                RefreshLanguageFromNativeUi();

            if (
                languageChanged ||
                ModSettingsApi.RegistryVersion !=
                _builtRegistryVersion
            )
            {
                RebuildModsPanel();
            }

            if (active && !_wasOptionsActive)
            {
                OnOptionsOpened();
            }

            _wasOptionsActive = active;

            if (active)
            {
                EnforcePanelIsolation();
                RefreshRuntimeControls();
            }
        }

        private Transform FindOptionsRoot()
        {
            MonoBehaviour[] behaviours =
                Resources.FindObjectsOfTypeAll<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                GameObject go =
                    behaviour.gameObject;

                if (!go.scene.IsValid())
                    continue;

                Type type =
                    behaviour.GetType();

                if (
                    type.FullName ==
                    "Beam.UI.OptionsMenuPresenter"
                )
                {
                    return behaviour.transform;
                }
            }

            return null;
        }

        private bool TryInject(
            Transform optionsRoot)
        {
            try
            {
                Transform generalButtonTransform =
                    FindDescendant(
                        optionsRoot,
                        "Button - General");

                if (generalButtonTransform == null)
                    return false;

                Transform buttonsGroup =
                    generalButtonTransform.parent;

                Transform aboutButtonTransform =
                    FindDirectChild(
                        buttonsGroup,
                        "Button - About");

                _generalPanel =
                    FindDescendant(
                        optionsRoot,
                        "Options - General");

                _graphicsPanel =
                    FindDescendant(
                        optionsRoot,
                        "Options - Graphics");

                _audioPanel =
                    FindDescendant(
                        optionsRoot,
                        "Options - Audio");

                _inputPanel =
                    FindDescendant(
                        optionsRoot,
                        "Options - Input");

                _aboutPanel =
                    FindDescendant(
                        optionsRoot,
                        "Options - About");

                if (
                    aboutButtonTransform == null ||
                    _generalPanel == null ||
                    _graphicsPanel == null ||
                    _audioPanel == null ||
                    _inputPanel == null ||
                    _aboutPanel == null
                )
                {
                    return false;
                }

                _content =
                    _generalPanel.parent;

                _scrollRect =
                    _content.GetComponentInParent<ScrollRect>();

                if (_scrollRect == null)
                    return false;

                _sourceCategory =
                    FindDescendant(
                        _generalPanel,
                        "Category - Interface");

                _sourceSlider =
                    FindDescendant(
                        _generalPanel,
                        "Control - Ocean Rotation");

                _sourceOptionButton =
                    FindDescendant(
                        _generalPanel,
                        "Control - Crosshair");

                if (
                    _sourceCategory == null ||
                    _sourceSlider == null ||
                    _sourceOptionButton == null
                )
                {
                    return false;
                }

                _nativePanels.Clear();
                _nativePanels.Add(_generalPanel);
                _nativePanels.Add(_graphicsPanel);
                _nativePanels.Add(_audioPanel);
                _nativePanels.Add(_inputPanel);
                _nativePanels.Add(_aboutPanel);

                _optionsRoot =
                    optionsRoot;

                _generalButtonTransform =
                    generalButtonTransform;

                RefreshLanguageFromNativeUi();

                CreateModsButton(
                    buttonsGroup,
                    aboutButtonTransform);

                CreateEmptyModsPanel();

                HookNativeButtons(
                    buttonsGroup);

                RebuildModsPanel();

                _wasOptionsActive =
                    _optionsRoot.gameObject.activeInHierarchy;

                Logger.LogInfo(
                    "Native MODS tab injected.");

                Logger.LogInfo(
                    "Registered mod settings are now rendered by the shared API.");

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    "Failed to inject Mod Settings: " + ex);

                _optionsRoot = null;
                return false;
            }
        }

        private void CreateModsButton(
            Transform buttonsGroup,
            Transform aboutButtonTransform)
        {
            Transform existing =
                FindDirectChild(
                    buttonsGroup,
                    "Button - Mods");

            if (existing != null)
            {
                _modsButton =
                    existing.GetComponent<Button>();

                UpdateModsButtonLabel();
                return;
            }

            GameObject clone =
                Instantiate(
                    aboutButtonTransform.gameObject,
                    buttonsGroup,
                    false);

            clone.name =
                "Button - Mods";

            clone.transform.SetSiblingIndex(
                aboutButtonTransform.GetSiblingIndex());

            DisableLocalizers(
                clone.transform);

            TMP_Text label =
                FindFirstText(
                    clone.transform);

            if (label != null)
            {
                label.text =
                    Localize(
                        "МОДЫ",
                        "MODS",
                        "MODS");
            }

            _modsButton =
                clone.GetComponent<Button>();

            if (_modsButton == null)
            {
                throw new Exception(
                    "Cloned MODS button has no Button.");
            }

            _modsButton.onClick =
                new Button.ButtonClickedEvent();

            _modsButton.onClick.AddListener(
                delegate
                {
                    ShowModsPanel();
                });
        }

        private void CreateEmptyModsPanel()
        {
            Transform existing =
                FindDirectChild(
                    _content,
                    "Options - Mods");

            if (existing != null)
            {
                _modsPanel =
                    existing.gameObject;

                return;
            }

            _modsPanel =
                Instantiate(
                    _generalPanel.gameObject,
                    _content,
                    false);

            _modsPanel.name =
                "Options - Mods";

            _modsPanel.SetActive(false);

            ClearChildrenImmediate(
                _modsPanel.transform);
        }

        private void RebuildModsPanel()
        {
            if (_modsPanel == null)
                return;

            ClearChildrenImmediate(
                _modsPanel.transform);

            _runtimeControls.Clear();

            List<ModDefinition> mods =
                ModSettingsApi.GetSnapshot();

            foreach (ModDefinition mod in mods)
            {
                if (
                    mod == null ||
                    mod.Settings.Count == 0
                )
                {
                    continue;
                }

                GameObject category =
                    CreateCategory(
                        Localize(
                            mod.DisplayNameRussian,
                            mod.DisplayNameEnglish,
                            mod.Id));

                foreach (
                    ModSettingDefinition setting
                    in mod.Settings)
                {
                    CreateSettingControl(
                        category.transform,
                        setting);
                }
            }

            _builtRegistryVersion =
                ModSettingsApi.RegistryVersion;

            RebuildAndScrollTop();

            Logger.LogInfo(
                "MODS page rebuilt. Registered mods=" +
                mods.Count +
                ", controls=" +
                _runtimeControls.Count +
                ".");
        }

        private GameObject CreateCategory(
            string title)
        {
            GameObject category =
                Instantiate(
                    _sourceCategory.gameObject,
                    _modsPanel.transform,
                    false);

            category.name =
                "Category - Mod - " +
                title;

            Transform categoryTransform =
                category.transform;

            for (
                int i =
                    categoryTransform.childCount - 1;
                i >= 0;
                i--)
            {
                Transform child =
                    categoryTransform.GetChild(i);

                if (
                    child.name.StartsWith(
                        "Control - ",
                        StringComparison.Ordinal)
                )
                {
                    DestroyImmediate(
                        child.gameObject);
                }
            }

            DisableLocalizers(
                categoryTransform);

            Transform titleObject =
                FindDescendant(
                    categoryTransform,
                    "Lable - Title");

            if (titleObject == null)
            {
                titleObject =
                    FindDescendant(
                        categoryTransform,
                        "Label - Title");
            }

            if (titleObject != null)
            {
                TMP_Text text =
                    titleObject.GetComponent<TMP_Text>();

                if (text != null)
                {
                    text.text =
                        title.ToUpper(
                            CultureInfo.CurrentCulture);
                }
            }

            return category;
        }

        private void CreateSettingControl(
            Transform category,
            ModSettingDefinition definition)
        {
            if (
                definition.Kind ==
                ModSettingKind.Slider)
            {
                CreateSliderControl(
                    category,
                    definition);

                return;
            }

            CreateButtonLikeControl(
                category,
                definition);
        }

        private void CreateSliderControl(
            Transform category,
            ModSettingDefinition definition)
        {
            GameObject control =
                Instantiate(
                    _sourceSlider.gameObject,
                    category,
                    false);

            control.name =
                "Control - Mod - " +
                definition.Id;

            DisableLocalizers(
                control.transform);

            DisableComponentByFullName(
                control,
                "Beam.UI.TMPOptionSliderViewAdapter");

            Slider slider =
                control.GetComponent<Slider>();

            if (slider == null)
            {
                DestroyImmediate(control);
                return;
            }

            slider.onValueChanged =
                new Slider.SliderEvent();

            slider.minValue =
                definition.Min;

            slider.maxValue =
                definition.Max;

            slider.wholeNumbers =
                false;

            RuntimeControl runtime =
                new RuntimeControl();

            runtime.Definition =
                definition;

            runtime.Slider =
                slider;

            runtime.NameLabel =
                FindTextByName(
                    control.transform,
                    "Control Name");

            runtime.ValueLabel =
                FindTextByName(
                    control.transform,
                    "Label - Slider Value");

            if (runtime.NameLabel != null)
            {
                runtime.NameLabel.text =
                    Localize(
                        definition.LabelRussian,
                        definition.LabelEnglish,
                        definition.Id);
            }

            slider.onValueChanged.AddListener(
                delegate(float raw)
                {
                    if (runtime.Suppress)
                        return;

                    float value =
                        SnapSliderValue(
                            definition,
                            raw);

                    if (
                        Math.Abs(
                            slider.value -
                            value) > 0.0001f
                    )
                    {
                        runtime.Suppress = true;
                        slider.value = value;
                        runtime.Suppress = false;
                    }

                    if (
                        definition.FloatSetter != null
                    )
                    {
                        definition.FloatSetter(
                            value);
                    }

                    UpdateRuntimeControl(
                        runtime);
                });

            AttachAutoScroll(control);

            _runtimeControls.Add(runtime);

            UpdateRuntimeControl(
                runtime);
        }

        private void CreateButtonLikeControl(
            Transform category,
            ModSettingDefinition definition)
        {
            GameObject control =
                Instantiate(
                    _sourceOptionButton.gameObject,
                    category,
                    false);

            control.name =
                "Control - Mod - " +
                definition.Id;

            DisableLocalizers(
                control.transform);

            DisableComponentByFullName(
                control,
                "Beam.UI.TMPOptionButtonViewAdapter");

            Button button =
                control.GetComponent<Button>();

            if (button == null)
            {
                DestroyImmediate(control);
                return;
            }

            button.onClick =
                new Button.ButtonClickedEvent();

            RuntimeControl runtime =
                new RuntimeControl();

            runtime.Definition =
                definition;

            runtime.Button =
                button;

            runtime.NameLabel =
                FindTextByName(
                    control.transform,
                    "Control Name");

            runtime.ValueLabel =
                FindTextByName(
                    control.transform,
                    "Label - Selection");

            if (runtime.NameLabel != null)
            {
                runtime.NameLabel.text =
                    Localize(
                        definition.LabelRussian,
                        definition.LabelEnglish,
                        definition.Id);
            }

            button.onClick.AddListener(
                delegate
                {
                    ActivateButtonSetting(
                        runtime);
                });

            AttachAutoScroll(control);

            _runtimeControls.Add(runtime);

            UpdateRuntimeControl(
                runtime);
        }

        private void AttachAutoScroll(
            GameObject control)
        {
            if (
                control == null ||
                _scrollRect == null
            )
            {
                return;
            }

            RectTransform target =
                control.GetComponent<RectTransform>();

            if (target == null)
                return;

            ScrollIntoViewOnSelect helper =
                control.GetComponent<ScrollIntoViewOnSelect>();

            if (helper == null)
            {
                helper =
                    control.AddComponent<ScrollIntoViewOnSelect>();
            }

            helper.Configure(
                _scrollRect,
                target);
        }

        private void ActivateButtonSetting(
            RuntimeControl runtime)
        {
            ModSettingDefinition definition =
                runtime.Definition;

            try
            {
                if (
                    definition.Kind ==
                    ModSettingKind.Toggle
                )
                {
                    if (
                        definition.BoolGetter == null ||
                        definition.BoolSetter == null
                    )
                    {
                        return;
                    }

                    bool value =
                        definition.BoolGetter();

                    definition.BoolSetter(
                        !value);
                }
                else if (
                    definition.Kind ==
                    ModSettingKind.Choice
                )
                {
                    if (
                        definition.ChoiceGetter == null ||
                        definition.ChoiceSetter == null ||
                        GetLocalizedChoices(definition).Length == 0
                    )
                    {
                        return;
                    }

                    int index =
                        definition.ChoiceGetter();

                    index++;

                    if (
                        index >=
                        GetLocalizedChoices(definition).Length
                    )
                    {
                        index = 0;
                    }

                    definition.ChoiceSetter(
                        index);
                }
                else if (
                    definition.Kind ==
                    ModSettingKind.Button
                )
                {
                    if (
                        definition.ButtonAction != null
                    )
                    {
                        definition.ButtonAction();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    "MODS control action failed: " +
                    definition.Id +
                    " :: " +
                    ex.Message);
            }

            UpdateRuntimeControl(
                runtime);
        }

        private void RefreshRuntimeControls()
        {
            for (
                int i = 0;
                i < _runtimeControls.Count;
                i++)
            {
                UpdateRuntimeControl(
                    _runtimeControls[i]);
            }
        }

        private void UpdateRuntimeControl(
            RuntimeControl runtime)
        {
            if (
                runtime == null ||
                runtime.Definition == null
            )
            {
                return;
            }

            ModSettingDefinition definition =
                runtime.Definition;

            try
            {
                if (
                    definition.Kind ==
                    ModSettingKind.Slider
                )
                {
                    bool valid =
                        definition.FloatGetter != null &&
                        definition.FloatSetter != null;

                    if (runtime.Slider != null)
                    {
                        runtime.Slider.interactable =
                            valid;
                    }

                    if (!valid)
                    {
                        SetValueText(
                            runtime,
                            Localize(
                                "НЕТ",
                                "N/A",
                                "N/A"));

                        return;
                    }

                    float value =
                        definition.FloatGetter();

                    value =
                        Mathf.Clamp(
                            value,
                            definition.Min,
                            definition.Max);

                    value =
                        SnapSliderValue(
                            definition,
                            value);

                    if (
                        runtime.Slider != null &&
                        Math.Abs(
                            runtime.Slider.value -
                            value) > 0.0001f
                    )
                    {
                        runtime.Suppress = true;
                        runtime.Slider.value = value;
                        runtime.Suppress = false;
                    }

                    SetValueText(
                        runtime,
                        FormatSliderValue(
                            definition,
                            value));

                    return;
                }

                if (
                    definition.Kind ==
                    ModSettingKind.Toggle
                )
                {
                    bool valid =
                        definition.BoolGetter != null &&
                        definition.BoolSetter != null;

                    if (runtime.Button != null)
                    {
                        runtime.Button.interactable =
                            valid;
                    }

                    if (!valid)
                    {
                        SetValueText(
                            runtime,
                            Localize(
                                "НЕТ",
                                "N/A",
                                "N/A"));

                        return;
                    }

                    bool value =
                        definition.BoolGetter();

                    SetValueText(
                        runtime,
                        value
                        ? Localize(
                            definition.OnTextRussian,
                            definition.OnTextEnglish,
                            "On")
                        : Localize(
                            definition.OffTextRussian,
                            definition.OffTextEnglish,
                            "Off"));

                    return;
                }

                if (
                    definition.Kind ==
                    ModSettingKind.Choice
                )
                {
                    bool valid =
                        definition.ChoiceGetter != null &&
                        definition.ChoiceSetter != null &&
                        GetLocalizedChoices(definition).Length > 0;

                    if (runtime.Button != null)
                    {
                        runtime.Button.interactable =
                            valid;
                    }

                    if (!valid)
                    {
                        SetValueText(
                            runtime,
                            Localize(
                                "НЕТ",
                                "N/A",
                                "N/A"));

                        return;
                    }

                    int index =
                        definition.ChoiceGetter();

                    if (index < 0)
                        index = 0;

                    if (
                        index >=
                        GetLocalizedChoices(definition).Length
                    )
                    {
                        index =
                            GetLocalizedChoices(definition).Length - 1;
                    }

                    SetValueText(
                        runtime,
                        GetLocalizedChoices(definition)[index]);

                    return;
                }

                if (
                    definition.Kind ==
                    ModSettingKind.Button
                )
                {
                    if (runtime.Button != null)
                    {
                        runtime.Button.interactable =
                            definition.ButtonAction != null;
                    }

                    SetValueText(
                        runtime,
                        Localize(
                            definition.ActionTextRussian,
                            definition.ActionTextEnglish,
                            "Run"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    "MODS control refresh failed: " +
                    definition.Id +
                    " :: " +
                    ex.Message);
            }
        }

        private void SetValueText(
            RuntimeControl runtime,
            string value)
        {
            if (runtime.ValueLabel == null)
                return;

            runtime.ValueLabel.text =
                value ?? "";
        }

        private float SnapSliderValue(
            ModSettingDefinition definition,
            float value)
        {
            value =
                Mathf.Clamp(
                    value,
                    definition.Min,
                    definition.Max);

            float step =
                definition.Step;

            if (step <= 0f)
                return value;

            float units =
                (value - definition.Min) /
                step;

            float snapped =
                definition.Min +
                Mathf.Round(units) *
                step;

            return Mathf.Clamp(
                snapped,
                definition.Min,
                definition.Max);
        }

        private string FormatSliderValue(
            ModSettingDefinition definition,
            float value)
        {
            float display =
                value *
                definition.DisplayMultiplier;

            string format =
                "F" +
                definition.Decimals;

            string text =
                display.ToString(
                    format,
                    CultureInfo.CurrentCulture);

            return text +
                Localize(
                    definition.SuffixRussian,
                    definition.SuffixEnglish,
                    "");
        }

        private void HookNativeButtons(
            Transform buttonsGroup)
        {
            HookNativeButton(
                buttonsGroup,
                "Button - General",
                _generalPanel);

            HookNativeButton(
                buttonsGroup,
                "Button - Graphics",
                _graphicsPanel);

            HookNativeButton(
                buttonsGroup,
                "Button - Audio",
                _audioPanel);

            HookNativeButton(
                buttonsGroup,
                "Button - Input",
                _inputPanel);

            HookNativeButton(
                buttonsGroup,
                "Button - About",
                _aboutPanel);
        }

        private void HookNativeButton(
            Transform buttonsGroup,
            string buttonName,
            Transform panel)
        {
            Transform buttonTransform =
                FindDirectChild(
                    buttonsGroup,
                    buttonName);

            if (buttonTransform == null)
                return;

            Button button =
                buttonTransform.GetComponent<Button>();

            if (button == null)
                return;

            button.onClick.AddListener(
                delegate
                {
                    ShowNativePanel(
                        panel);
                });
        }

        private void ShowModsPanel()
        {
            if (_modsPanel == null)
                return;

            foreach (Transform panel in _nativePanels)
            {
                if (panel != null)
                {
                    panel.gameObject.SetActive(false);
                }
            }

            _modsPanel.SetActive(true);

            RefreshRuntimeControls();
            RebuildAndScrollTop();

            Logger.LogInfo(
                "MODS panel opened.");
        }

        private void ShowNativePanel(
            Transform panelToShow)
        {
            if (_modsPanel != null)
            {
                _modsPanel.SetActive(false);
            }

            foreach (Transform panel in _nativePanels)
            {
                if (panel != null)
                {
                    panel.gameObject.SetActive(
                        panel == panelToShow);
                }
            }

            RebuildAndScrollTop();
        }

        private void EnforcePanelIsolation()
        {
            if (
                _modsPanel == null ||
                !_modsPanel.activeSelf
            )
            {
                _panelOverlapLogged = false;
                return;
            }

            foreach (Transform panel in _nativePanels)
            {
                if (
                    panel == null ||
                    !panel.gameObject.activeSelf
                )
                {
                    continue;
                }

                _modsPanel.SetActive(false);

                if (!_panelOverlapLogged)
                {
                    Logger.LogWarning(
                        "Native Options panel became active while MODS was active; " +
                        "hiding MODS panel to prevent mixed scroll content.");

                    _panelOverlapLogged = true;
                }

                RebuildAndScrollTop();
                return;
            }

            _panelOverlapLogged = false;
        }

        private void OnOptionsOpened()
        {
            if (_modsPanel != null)
            {
                _modsPanel.SetActive(false);
            }

            bool anyNativeActive = false;

            foreach (Transform panel in _nativePanels)
            {
                if (
                    panel != null &&
                    panel.gameObject.activeSelf
                )
                {
                    anyNativeActive = true;
                    break;
                }
            }

            if (
                !anyNativeActive &&
                _generalPanel != null
            )
            {
                _generalPanel.gameObject.SetActive(true);
            }

            if (RefreshLanguageFromNativeUi())
            {
                RebuildModsPanel();
                return;
            }

            RefreshRuntimeControls();
            RebuildAndScrollTop();
        }

        private bool RefreshLanguageFromNativeUi()
        {
            if (_generalButtonTransform == null)
                return false;

            TMP_Text label =
                FindFirstText(
                    _generalButtonTransform);

            if (label == null || String.IsNullOrEmpty(label.text))
                return false;

            bool russian =
                ContainsCyrillic(label.text);

            bool changed =
                !_languageInitialized ||
                russian != _useRussian;

            if (!changed)
                return false;

            _languageInitialized = true;
            _useRussian = russian;

            UpdateModsButtonLabel();

            Logger.LogInfo(
                "Options language detected: " +
                (_useRussian ? "RU" : "EN") +
                ".");

            return true;
        }

        private void UpdateModsButtonLabel()
        {
            if (_modsButton == null)
                return;

            TMP_Text label =
                FindFirstText(
                    _modsButton.transform);

            if (label != null)
            {
                label.text =
                    Localize(
                        "МОДЫ",
                        "MODS",
                        "MODS");
            }
        }

        private string Localize(
            string russian,
            string english,
            string fallback)
        {
            string primary =
                _useRussian
                ? russian
                : english;

            string secondary =
                _useRussian
                ? english
                : russian;

            if (!String.IsNullOrEmpty(primary))
                return primary;

            if (!String.IsNullOrEmpty(secondary))
                return secondary;

            return fallback ?? "";
        }

        private string[] GetLocalizedChoices(
            ModSettingDefinition definition)
        {
            if (definition == null)
                return new string[0];

            string[] primary =
                _useRussian
                ? definition.ChoicesRussian
                : definition.ChoicesEnglish;

            string[] secondary =
                _useRussian
                ? definition.ChoicesEnglish
                : definition.ChoicesRussian;

            if (primary != null && primary.Length > 0)
                return primary;

            if (secondary != null)
                return secondary;

            return new string[0];
        }

        private bool ContainsCyrillic(
            string text)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (
                    (c >= '\u0400' && c <= '\u04FF') ||
                    (c >= '\u0500' && c <= '\u052F')
                )
                {
                    return true;
                }
            }

            return false;
        }

        private void RebuildAndScrollTop()
        {
            Canvas.ForceUpdateCanvases();

            if (_content != null)
            {
                RectTransform rect =
                    _content.GetComponent<RectTransform>();

                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(
                        rect);
                }
            }

            if (
                _modsPanel != null &&
                _modsPanel.activeSelf
            )
            {
                RectTransform rect =
                    _modsPanel.GetComponent<RectTransform>();

                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(
                        rect);
                }
            }

            Canvas.ForceUpdateCanvases();

            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition =
                    1f;
            }
        }

        private void ClearChildrenImmediate(
            Transform parent)
        {
            if (parent == null)
                return;

            for (
                int i = parent.childCount - 1;
                i >= 0;
                i--)
            {
                DestroyImmediate(
                    parent.GetChild(i).gameObject);
            }
        }

        private void DisableLocalizers(
            Transform root)
        {
            Component[] components =
                root.GetComponentsInChildren<Component>(
                    true);

            foreach (Component component in components)
            {
                if (component == null)
                    continue;

                if (
                    component.GetType().FullName !=
                    "Beam.Language.Localizer"
                )
                {
                    continue;
                }

                Behaviour behaviour =
                    component as Behaviour;

                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }

                DestroyImmediate(component);
            }
        }

        private void DisableComponentByFullName(
            GameObject gameObject,
            string fullName)
        {
            Component[] components =
                gameObject.GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component == null)
                    continue;

                if (
                    component.GetType().FullName !=
                    fullName
                )
                {
                    continue;
                }

                Behaviour behaviour =
                    component as Behaviour;

                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }

                DestroyImmediate(component);
            }
        }

        private TMP_Text FindTextByName(
            Transform root,
            string transformName)
        {
            Transform t =
                FindDescendant(
                    root,
                    transformName);

            if (t == null)
                return null;

            return t.GetComponent<TMP_Text>();
        }

        private TMP_Text FindFirstText(
            Transform root)
        {
            TMP_Text[] texts =
                root.GetComponentsInChildren<TMP_Text>(
                    true);

            if (
                texts == null ||
                texts.Length == 0
            )
            {
                return null;
            }

            return texts[0];
        }

        private Transform FindDescendant(
            Transform root,
            string name)
        {
            if (root == null)
                return null;

            if (root.name == name)
                return root;

            for (
                int i = 0;
                i < root.childCount;
                i++
            )
            {
                Transform result =
                    FindDescendant(
                        root.GetChild(i),
                        name);

                if (result != null)
                    return result;
            }

            return null;
        }

        private Transform FindDirectChild(
            Transform parent,
            string name)
        {
            if (parent == null)
                return null;

            for (
                int i = 0;
                i < parent.childCount;
                i++
            )
            {
                Transform child =
                    parent.GetChild(i);

                if (child.name == name)
                    return child;
            }

            return null;
        }
    }
}
