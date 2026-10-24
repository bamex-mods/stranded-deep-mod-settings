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
        Button = 3,
        Header = 4
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

        public static void AddHeader(
            string modId,
            string headerId,
            string title,
            int order)
        {
            AddHeaderLocalized(
                modId,
                headerId,
                title,
                title,
                order);
        }

        public static void AddHeaderLocalized(
            string modId,
            string headerId,
            string titleRussian,
            string titleEnglish,
            int order)
        {
            ModSettingDefinition header =
                NewSetting(
                    modId,
                    headerId,
                    titleRussian,
                    titleEnglish,
                    order,
                    ModSettingKind.Header);

            UpsertSetting(
                modId,
                header);
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
                    SameEntryIdentity(
                        mod.Settings[i],
                        setting)
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

        private static bool SameEntryIdentity(
            ModSettingDefinition existing,
            ModSettingDefinition incoming)
        {
            if (existing == null || incoming == null)
                return false;

            if (
                existing.Kind == ModSettingKind.Header ||
                incoming.Kind == ModSettingKind.Header
            )
            {
                return
                    existing.Kind == incoming.Kind &&
                    String.Equals(
                        existing.Id,
                        incoming.Id,
                        StringComparison.OrdinalIgnoreCase);
            }

            return String.Equals(
                existing.Id,
                incoming.Id,
                StringComparison.OrdinalIgnoreCase);
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

    internal class ModSelectorRuntime
    {
        public string ModId;
        public Button Button;
        public TMP_Text Label;
    }

    internal class ModSelectorOnSelect : MonoBehaviour, ISelectHandler
    {
        private string _modId;
        private Action<string> _callback;

        public void Configure(
            string modId,
            Action<string> callback)
        {
            _modId = modId;
            _callback = callback;
        }

        public void OnSelect(
            BaseEventData eventData)
        {
            if (_callback != null)
            {
                _callback(_modId);
            }
        }
    }

    internal class ReturnToModSelectorOnCancel :
        MonoBehaviour,
        ICancelHandler
    {
        private Func<Selectable> _targetGetter;

        public void Configure(
            Func<Selectable> targetGetter)
        {
            _targetGetter = targetGetter;
        }

        public void OnCancel(
            BaseEventData eventData)
        {
            if (_targetGetter == null)
                return;

            Selectable target =
                _targetGetter();

            if (target == null)
                return;

            target.Select();

            if (eventData != null)
                eventData.Use();
        }
    }

    [BepInPlugin(
        "com.bamex.strandeddeep.modsettings",
        "Stranded Deep Mod Settings",
        "0.4.3")]
    public class ModSettingsPlugin : BaseUnityPlugin
    {
        private const float ScanInterval = 0.50f;
        private const float ModSelectorWidth = 250f;
        private const float ModSelectorGap = 24f;
        private const float ColumnDividerWidth = 2f;
        private const string AllModsId = "__all__";

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

        private readonly List<ModSelectorRuntime> _modSelectorButtons =
            new List<ModSelectorRuntime>();

        private readonly Dictionary<string, float> _settingsScrollPositions =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        private GameObject _modsPanel;
        private GameObject _modSelectorRoot;
        private GameObject _modSelectorDivider;
        private Transform _modSelectorContent;
        private ScrollRect _modSelectorScrollRect;
        private RectTransform _optionsScrollRectTransform;
        private Vector2 _optionsScrollOriginalAnchoredPosition;
        private Vector2 _optionsScrollOriginalSizeDelta;
        private bool _optionsScrollGeometryCaptured;
        private bool _modsLayoutApplied;
        private string _selectedModId = AllModsId;
        private Navigation _modsButtonOriginalNavigation;
        private bool _modsButtonNavigationCaptured;
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
                "Stranded Deep Mod Settings API v0.4.3 loaded.");

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
                CreateModSelectorPanel();

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

            List<ModDefinition> mods =
                ModSettingsApi.GetSnapshot();

            EnsureSelectedModStillExists(mods);
            RebuildModSelector(mods);
            RebuildSettingsContent(mods, true);

            _builtRegistryVersion =
                ModSettingsApi.RegistryVersion;

            Logger.LogInfo(
                "MODS UI rebuilt. Registered mods=" +
                mods.Count +
                ", filter=" +
                _selectedModId +
                ", controls=" +
                _runtimeControls.Count +
                ".");
        }

        private void EnsureSelectedModStillExists(
            List<ModDefinition> mods)
        {
            if (
                String.IsNullOrEmpty(_selectedModId) ||
                String.Equals(
                    _selectedModId,
                    AllModsId,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                _selectedModId = AllModsId;
                return;
            }

            foreach (ModDefinition mod in mods)
            {
                if (
                    mod != null &&
                    String.Equals(
                        mod.Id,
                        _selectedModId,
                        StringComparison.OrdinalIgnoreCase)
                )
                {
                    return;
                }
            }

            _selectedModId = AllModsId;
        }

        private void RebuildSettingsContent(
            List<ModDefinition> mods,
            bool preserveScroll)
        {
            if (_modsPanel == null)
                return;

            float previousScroll = 1f;

            if (
                preserveScroll &&
                _scrollRect != null
            )
            {
                previousScroll =
                    _scrollRect.verticalNormalizedPosition;
            }

            ClearChildrenImmediate(
                _modsPanel.transform);

            _runtimeControls.Clear();

            foreach (ModDefinition mod in mods)
            {
                if (
                    mod == null ||
                    mod.Settings.Count == 0 ||
                    !ShouldRenderMod(mod.Id)
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
                    if (setting == null)
                        continue;

                    if (
                        setting.Kind ==
                        ModSettingKind.Header
                    )
                    {
                        CreateSubHeader(
                            category.transform,
                            setting);
                    }
                    else
                    {
                        CreateSettingControl(
                            category.transform,
                            setting);
                    }
                }
            }

            RebuildLayouts();

            if (_scrollRect != null)
            {
                float targetScroll = 1f;
                float stored;

                if (
                    preserveScroll &&
                    _settingsScrollPositions.TryGetValue(
                        _selectedModId,
                        out stored)
                )
                {
                    targetScroll = stored;
                }
                else if (preserveScroll)
                {
                    targetScroll = previousScroll;
                }

                _scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(targetScroll);
            }

            ConfigureColumnNavigation();
        }

        private bool ShouldRenderMod(
            string modId)
        {
            return
                String.Equals(
                    _selectedModId,
                    AllModsId,
                    StringComparison.OrdinalIgnoreCase) ||
                String.Equals(
                    _selectedModId,
                    modId,
                    StringComparison.OrdinalIgnoreCase);
        }

        private void SelectModFilter(
            string modId)
        {
            string normalized =
                String.IsNullOrEmpty(modId)
                ? AllModsId
                : modId;

            if (
                String.Equals(
                    normalized,
                    _selectedModId,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return;
            }

            if (_scrollRect != null)
            {
                _settingsScrollPositions[_selectedModId] =
                    _scrollRect.verticalNormalizedPosition;
            }

            float targetScroll = 1f;
            float stored;

            if (
                _settingsScrollPositions.TryGetValue(
                    normalized,
                    out stored)
            )
            {
                targetScroll = stored;
            }

            _selectedModId = normalized;

            List<ModDefinition> mods =
                ModSettingsApi.GetSnapshot();

            EnsureSelectedModStillExists(mods);
            RebuildSettingsContent(mods, false);

            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(targetScroll);
            }

            Logger.LogInfo(
                "MODS filter selected: " +
                _selectedModId +
                ".");
        }

        private void CreateModSelectorPanel()
        {
            if (
                _scrollRect == null ||
                _modsButton == null ||
                _modSelectorRoot != null
            )
            {
                return;
            }

            _optionsScrollRectTransform =
                _scrollRect.GetComponent<RectTransform>();

            if (_optionsScrollRectTransform == null)
                return;

            GameObject root =
                new GameObject(
                    "Mod Settings - Mod Selector",
                    typeof(RectTransform),
                    typeof(ScrollRect));

            root.transform.SetParent(
                _optionsScrollRectTransform.parent,
                false);

            RectTransform rootRect =
                root.GetComponent<RectTransform>();

            CopyRectTransformGeometry(
                _optionsScrollRectTransform,
                rootRect);

            ScrollRect selectorScroll =
                root.GetComponent<ScrollRect>();

            selectorScroll.horizontal = false;
            selectorScroll.vertical = true;
            selectorScroll.movementType =
                ScrollRect.MovementType.Clamped;
            selectorScroll.inertia = true;
            selectorScroll.scrollSensitivity = 28f;

            GameObject viewport =
                new GameObject(
                    "Viewport",
                    typeof(RectTransform),
                    typeof(RectMask2D));

            viewport.transform.SetParent(
                root.transform,
                false);

            RectTransform viewportRect =
                viewport.GetComponent<RectTransform>();

            StretchToParent(viewportRect);

            GameObject content =
                new GameObject(
                    "Content",
                    typeof(RectTransform),
                    typeof(VerticalLayoutGroup),
                    typeof(ContentSizeFitter));

            content.transform.SetParent(
                viewport.transform,
                false);

            RectTransform contentRect =
                content.GetComponent<RectTransform>();

            contentRect.anchorMin =
                new Vector2(0f, 1f);
            contentRect.anchorMax =
                new Vector2(1f, 1f);
            contentRect.pivot =
                new Vector2(0.5f, 1f);
            contentRect.anchoredPosition =
                Vector2.zero;
            contentRect.sizeDelta =
                new Vector2(0f, 0f);

            VerticalLayoutGroup layout =
                content.GetComponent<VerticalLayoutGroup>();

            layout.padding =
                new RectOffset(0, 8, 0, 0);
            layout.spacing = 10f;
            layout.childAlignment =
                TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter =
                content.GetComponent<ContentSizeFitter>();

            fitter.horizontalFit =
                ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            selectorScroll.viewport =
                viewportRect;
            selectorScroll.content =
                contentRect;

            _modSelectorRoot = root;
            _modSelectorScrollRect = selectorScroll;
            _modSelectorContent = content.transform;

            CreateColumnDivider();

            root.SetActive(false);
            if (_modSelectorDivider != null)
                _modSelectorDivider.SetActive(false);
        }

        private void RebuildModSelector(
            List<ModDefinition> mods)
        {
            if (_modSelectorContent == null)
                return;

            ClearChildrenImmediate(
                _modSelectorContent);

            _modSelectorButtons.Clear();

            CreateModSelectorButton(
                AllModsId,
                Localize(
                    "ВСЕ МОДЫ",
                    "ALL MODS",
                    "ALL MODS"));

            foreach (ModDefinition mod in mods)
            {
                if (
                    mod == null ||
                    mod.Settings.Count == 0
                )
                {
                    continue;
                }

                CreateModSelectorButton(
                    mod.Id,
                    Localize(
                        mod.DisplayNameRussian,
                        mod.DisplayNameEnglish,
                        mod.Id));
            }

            RebuildSelectorLayout();
            ConfigureColumnNavigation();
        }

        private void CreateModSelectorButton(
            string modId,
            string title)
        {
            if (
                _modsButton == null ||
                _modSelectorContent == null
            )
            {
                return;
            }

            GameObject clone =
                Instantiate(
                    _modsButton.gameObject,
                    _modSelectorContent,
                    false);

            clone.name =
                "Button - Mod Selector - " +
                modId;

            DisableLocalizers(
                clone.transform);

            DisableBeamUiBehaviours(
                clone);

            Button button =
                clone.GetComponent<Button>();

            if (button == null)
            {
                DestroyImmediate(clone);
                return;
            }

            button.onClick =
                new Button.ButtonClickedEvent();

            string capturedModId = modId;

            button.onClick.AddListener(
                delegate
                {
                    SelectModFilter(
                        capturedModId);
                });

            TMP_Text label =
                FindFirstText(
                    clone.transform);

            if (label != null)
            {
                label.text =
                    title.ToUpper(
                        CultureInfo.CurrentCulture);

                ConfigureModSelectorLabel(
                    label);
            }

            LayoutElement element =
                clone.GetComponent<LayoutElement>();

            if (element == null)
            {
                element =
                    clone.AddComponent<LayoutElement>();
            }

            float currentHeight =
                clone.GetComponent<RectTransform>() != null
                ? clone.GetComponent<RectTransform>().rect.height
                : 0f;

            element.preferredHeight =
                Mathf.Max(
                    currentHeight,
                    64f);
            element.flexibleWidth = 1f;

            ModSelectorOnSelect select =
                clone.GetComponent<ModSelectorOnSelect>();

            if (select == null)
            {
                select =
                    clone.AddComponent<ModSelectorOnSelect>();
            }

            select.Configure(
                modId,
                SelectModFilter);

            AttachAutoScroll(
                clone,
                _modSelectorScrollRect);

            ModSelectorRuntime runtime =
                new ModSelectorRuntime();

            runtime.ModId = modId;
            runtime.Button = button;
            runtime.Label = label;

            _modSelectorButtons.Add(
                runtime);
        }

        private void ApplyModsColumnLayout()
        {
            if (
                _modsLayoutApplied ||
                _optionsScrollRectTransform == null ||
                _modSelectorRoot == null
            )
            {
                return;
            }

            _optionsScrollOriginalAnchoredPosition =
                _optionsScrollRectTransform.anchoredPosition;

            _optionsScrollOriginalSizeDelta =
                _optionsScrollRectTransform.sizeDelta;

            _optionsScrollGeometryCaptured = true;

            RectTransform selectorRect =
                _modSelectorRoot.GetComponent<RectTransform>();

            if (selectorRect != null)
            {
                CopyRectTransformGeometry(
                    _optionsScrollRectTransform,
                    selectorRect);

                SetRectWidthKeepingLeft(
                    selectorRect,
                    ModSelectorWidth);
            }

            PositionColumnDivider();

            float shift =
                ModSelectorWidth +
                ModSelectorGap;

            Vector2 size =
                _optionsScrollOriginalSizeDelta;

            size.x -= shift;

            Vector2 position =
                _optionsScrollOriginalAnchoredPosition;

            position.x +=
                (1f - _optionsScrollRectTransform.pivot.x) *
                shift;

            _optionsScrollRectTransform.sizeDelta = size;
            _optionsScrollRectTransform.anchoredPosition = position;

            _modSelectorRoot.SetActive(true);
            if (_modSelectorDivider != null)
                _modSelectorDivider.SetActive(true);

            _modsLayoutApplied = true;

            Canvas.ForceUpdateCanvases();

            RectTransform selectorLogRect =
                _modSelectorRoot.GetComponent<RectTransform>();

            Logger.LogInfo(
                "MODS three-column layout applied. selectorWidth=" +
                (selectorLogRect != null
                    ? selectorLogRect.rect.width.ToString("F1", CultureInfo.InvariantCulture)
                    : "n/a") +
                ", settingsWidth=" +
                _optionsScrollRectTransform.rect.width.ToString(
                    "F1",
                    CultureInfo.InvariantCulture) +
                ".");

            ConfigureColumnNavigation();
        }

        private void RestoreNativeColumnLayout()
        {
            if (!_optionsScrollGeometryCaptured)
                return;

            if (_optionsScrollRectTransform != null)
            {
                _optionsScrollRectTransform.anchoredPosition =
                    _optionsScrollOriginalAnchoredPosition;

                _optionsScrollRectTransform.sizeDelta =
                    _optionsScrollOriginalSizeDelta;
            }

            if (_modSelectorRoot != null)
            {
                _modSelectorRoot.SetActive(false);
            }

            if (_modSelectorDivider != null)
            {
                _modSelectorDivider.SetActive(false);
            }

            RestoreModsButtonNavigation();
            _modsLayoutApplied = false;
        }

        private void ConfigureColumnNavigation()
        {
            if (_modSelectorButtons.Count == 0)
                return;

            Selectable firstRight =
                GetFirstRuntimeSelectable();

            for (
                int i = 0;
                i < _modSelectorButtons.Count;
                i++)
            {
                ModSelectorRuntime current =
                    _modSelectorButtons[i];

                if (
                    current == null ||
                    current.Button == null
                )
                {
                    continue;
                }

                Navigation navigation =
                    new Navigation();

                navigation.mode =
                    Navigation.Mode.Explicit;

                if (i > 0)
                {
                    navigation.selectOnUp =
                        _modSelectorButtons[i - 1].Button;
                }

                if (
                    i + 1 <
                    _modSelectorButtons.Count
                )
                {
                    navigation.selectOnDown =
                        _modSelectorButtons[i + 1].Button;
                }

                navigation.selectOnLeft =
                    _modsButton;

                navigation.selectOnRight =
                    firstRight;

                current.Button.navigation =
                    navigation;
            }

            ConfigureModsButtonRightNavigation();
        }

        private void ConfigureModsButtonRightNavigation()
        {
            if (
                !_modsLayoutApplied ||
                _modsButton == null ||
                _modSelectorButtons.Count == 0
            )
            {
                return;
            }

            if (!_modsButtonNavigationCaptured)
            {
                _modsButtonOriginalNavigation =
                    _modsButton.navigation;
                _modsButtonNavigationCaptured = true;
            }

            Transform buttonsGroup =
                _modsButton.transform.parent;

            Button previous =
                FindSiblingButton(
                    buttonsGroup,
                    _modsButton.transform,
                    -1);

            Button next =
                FindSiblingButton(
                    buttonsGroup,
                    _modsButton.transform,
                    1);

            Navigation navigation =
                new Navigation();

            navigation.mode =
                Navigation.Mode.Explicit;
            navigation.selectOnUp = previous;
            navigation.selectOnDown = next;
            navigation.selectOnRight =
                GetSelectedModSelectorButton();

            _modsButton.navigation = navigation;
        }

        private void RestoreModsButtonNavigation()
        {
            if (
                _modsButton != null &&
                _modsButtonNavigationCaptured
            )
            {
                _modsButton.navigation =
                    _modsButtonOriginalNavigation;
            }
        }

        private Button FindSiblingButton(
            Transform parent,
            Transform current,
            int direction)
        {
            if (parent == null || current == null)
                return null;

            int index =
                current.GetSiblingIndex() + direction;

            while (
                index >= 0 &&
                index < parent.childCount
            )
            {
                Transform candidate =
                    parent.GetChild(index);

                if (
                    candidate != null &&
                    candidate.gameObject.activeInHierarchy
                )
                {
                    Button button =
                        candidate.GetComponent<Button>();

                    if (button != null)
                        return button;
                }

                index += direction;
            }

            return null;
        }

        private Button GetSelectedModSelectorButton()
        {
            foreach (
                ModSelectorRuntime runtime
                in _modSelectorButtons)
            {
                if (
                    runtime != null &&
                    runtime.Button != null &&
                    String.Equals(
                        runtime.ModId,
                        _selectedModId,
                        StringComparison.OrdinalIgnoreCase)
                )
                {
                    return runtime.Button;
                }
            }

            if (
                _modSelectorButtons.Count > 0 &&
                _modSelectorButtons[0] != null
            )
            {
                return _modSelectorButtons[0].Button;
            }

            return null;
        }

        private Selectable GetFirstRuntimeSelectable()
        {
            foreach (
                RuntimeControl runtime
                in _runtimeControls)
            {
                if (runtime == null)
                    continue;

                if (runtime.Slider != null)
                    return runtime.Slider;

                if (runtime.Button != null)
                    return runtime.Button;
            }

            return null;
        }

        private void AttachReturnToSelector(
            GameObject control)
        {
            if (control == null)
                return;

            ReturnToModSelectorOnCancel helper =
                control.GetComponent<ReturnToModSelectorOnCancel>();

            if (helper == null)
            {
                helper =
                    control.AddComponent<ReturnToModSelectorOnCancel>();
            }

            helper.Configure(
                delegate
                {
                    return GetSelectedModSelectorButton();
                });
        }

        private void CreateSubHeader(
            Transform category,
            ModSettingDefinition definition)
        {
            if (
                category == null ||
                definition == null ||
                _sourceCategory == null
            )
            {
                return;
            }

            bool firstSubHeader =
                !HasRenderedSettingsElement(
                    category);

            CreateVerticalSpacer(
                category,
                firstSubHeader ? 4f : 18f,
                "Before Header - " + definition.Id);

            GameObject header =
                Instantiate(
                    _sourceCategory.gameObject,
                    category,
                    false);

            header.name =
                "Header - Mod - " +
                definition.Id;

            Transform headerTransform =
                header.transform;

            for (
                int i =
                    headerTransform.childCount - 1;
                i >= 0;
                i--)
            {
                Transform child =
                    headerTransform.GetChild(i);

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
                headerTransform);

            Transform titleObject =
                FindDescendant(
                    headerTransform,
                    "Lable - Title");

            if (titleObject == null)
            {
                titleObject =
                    FindDescendant(
                        headerTransform,
                        "Label - Title");
            }

            if (titleObject != null)
            {
                TMP_Text text =
                    titleObject.GetComponent<TMP_Text>();

                if (text != null)
                {
                    text.text =
                        Localize(
                            definition.LabelRussian,
                            definition.LabelEnglish,
                            definition.Id)
                            .ToUpper(
                                CultureInfo.CurrentCulture);

                    text.fontStyle =
                        FontStyles.Bold;

                    text.fontSize =
                        text.fontSize * 0.80f;

                    text.enableWordWrapping = true;
                    text.overflowMode =
                        TextOverflowModes.Overflow;
                }
            }

            CanvasGroup group =
                header.GetComponent<CanvasGroup>();

            if (group == null)
            {
                group =
                    header.AddComponent<CanvasGroup>();
            }

            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;

            CreateVerticalSpacer(
                category,
                5f,
                "After Header - " + definition.Id);
        }

        private bool HasRenderedSettingsElement(
            Transform category)
        {
            if (category == null)
                return false;

            for (int i = 0; i < category.childCount; i++)
            {
                Transform child =
                    category.GetChild(i);

                if (child == null)
                    continue;

                if (
                    child.name.StartsWith(
                        "Control - Mod - ",
                        StringComparison.Ordinal) ||
                    child.name.StartsWith(
                        "Header - Mod - ",
                        StringComparison.Ordinal)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateVerticalSpacer(
            Transform parent,
            float height,
            string objectName)
        {
            if (
                parent == null ||
                height <= 0f
            )
            {
                return;
            }

            GameObject spacer =
                new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(LayoutElement));

            spacer.transform.SetParent(
                parent,
                false);

            LayoutElement layout =
                spacer.GetComponent<LayoutElement>();

            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleHeight = 0f;
            layout.ignoreLayout = false;
        }

        private void CreateColumnDivider()
        {
            if (
                _optionsScrollRectTransform == null ||
                _modSelectorDivider != null
            )
            {
                return;
            }

            GameObject divider =
                new GameObject(
                    "Mod Settings - Column Divider",
                    typeof(RectTransform),
                    typeof(Image));

            divider.transform.SetParent(
                _optionsScrollRectTransform.parent,
                false);

            Image dividerImage =
                divider.GetComponent<Image>();

            Transform sourceSeparator =
                FindDescendant(
                    _sourceCategory,
                    "Image - Separator");

            Image sourceImage =
                sourceSeparator != null
                ? sourceSeparator.GetComponent<Image>()
                : null;

            if (sourceImage != null)
            {
                dividerImage.sprite = sourceImage.sprite;
                dividerImage.material = sourceImage.material;
                dividerImage.type = sourceImage.type;

                Color color = sourceImage.color;
                color.a = Mathf.Clamp01(
                    color.a * 0.70f);
                dividerImage.color = color;
            }
            else
            {
                dividerImage.color =
                    new Color(1f, 1f, 1f, 0.24f);
            }

            dividerImage.raycastTarget = false;

            _modSelectorDivider = divider;
        }

        private void PositionColumnDivider()
        {
            if (
                _modSelectorDivider == null ||
                _optionsScrollRectTransform == null
            )
            {
                return;
            }

            RectTransform dividerRect =
                _modSelectorDivider.GetComponent<RectTransform>();

            if (dividerRect == null)
                return;

            CopyRectTransformGeometry(
                _optionsScrollRectTransform,
                dividerRect);

            SetRectWidthKeepingLeft(
                dividerRect,
                ColumnDividerWidth);

            Vector2 position =
                dividerRect.anchoredPosition;

            position.x +=
                ModSelectorWidth +
                ((ModSelectorGap - ColumnDividerWidth) * 0.5f);

            dividerRect.anchoredPosition = position;
        }

        private void ConfigureModSelectorLabel(
            TMP_Text text)
        {
            if (text == null)
                return;

            // The middle MODS column is navigation, not a settings row.
            // Give long mod names up to two visual lines instead of
            // hiding them behind ellipsis. The viewport mask remains
            // the final safety boundary for unusually long names.
            text.enableWordWrapping = true;
            text.overflowMode =
                TextOverflowModes.Truncate;
            text.maskable = true;
            text.fontStyle = FontStyles.Normal;
            text.fontSize =
                text.fontSize * 0.82f;

            Vector4 margin = text.margin;
            margin.x = 0f;
            margin.z = 0f;
            text.margin = margin;

            RectTransform textRect =
                text.rectTransform;

            if (textRect != null)
            {
                Vector2 min = textRect.offsetMin;

                // Native Options buttons reserve a large left inset.
                // It is useful in the stock first column but wastes
                // space in our narrower mod-selector column.
                min.x = Mathf.Max(
                    8f,
                    min.x - 52f);

                textRect.offsetMin = min;
            }

            LayoutElement layout =
                text.GetComponent<LayoutElement>();

            if (layout == null)
            {
                layout =
                    text.gameObject.AddComponent<LayoutElement>();
            }

            layout.minWidth = 0f;
            layout.preferredWidth = 0f;
            layout.flexibleWidth = 1f;
        }

        private void ConfigureWrappedSettingLabel(
            GameObject control,
            TMP_Text text)
        {
            if (
                control == null ||
                text == null
            )
            {
                return;
            }

            text.enableWordWrapping = true;
            text.overflowMode =
                TextOverflowModes.Overflow;
            text.maskable = true;

            LayoutElement textLayout =
                text.GetComponent<LayoutElement>();

            if (textLayout == null)
            {
                textLayout =
                    text.gameObject.AddComponent<LayoutElement>();
            }

            textLayout.minWidth = 0f;
            textLayout.preferredWidth = 0f;
            textLayout.flexibleWidth = 1f;

            RectTransform controlRect =
                control.GetComponent<RectTransform>();

            RectTransform textRect =
                text.rectTransform;

            if (
                controlRect == null ||
                textRect == null
            )
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                controlRect);

            float availableWidth =
                textRect.rect.width;

            if (availableWidth <= 1f)
                return;

            text.ForceMeshUpdate();

            int lineCount =
                text.textInfo != null
                ? Mathf.Max(1, text.textInfo.lineCount)
                : 1;

            Vector2 preferred =
                text.GetPreferredValues(
                    text.text,
                    availableWidth,
                    4096f);

            float nativeTextHeight =
                Mathf.Max(
                    1f,
                    textRect.rect.height);

            float targetTextHeight =
                Mathf.Max(
                    nativeTextHeight,
                    preferred.y);

            if (lineCount > 1)
            {
                float lineHeight =
                    Mathf.Max(
                        text.fontSize * 1.15f,
                        targetTextHeight / lineCount);

                targetTextHeight =
                    Mathf.Max(
                        targetTextHeight,
                        lineHeight * lineCount);
            }

            textLayout.minHeight =
                targetTextHeight;
            textLayout.preferredHeight =
                targetTextHeight;
            textLayout.flexibleHeight = 0f;

            if (lineCount <= 1)
                return;

            LayoutElement rowLayout =
                control.GetComponent<LayoutElement>();

            if (rowLayout == null)
            {
                rowLayout =
                    control.AddComponent<LayoutElement>();
            }

            float nativeRowHeight =
                controlRect.rect.height;

            if (rowLayout.preferredHeight > 0f)
            {
                nativeRowHeight =
                    Mathf.Max(
                        nativeRowHeight,
                        rowLayout.preferredHeight);
            }

            float extraHeight =
                Mathf.Max(
                    0f,
                    targetTextHeight - nativeTextHeight);

            float targetRowHeight =
                nativeRowHeight +
                extraHeight +
                6f;

            rowLayout.minHeight =
                targetRowHeight;
            rowLayout.preferredHeight =
                targetRowHeight;
            rowLayout.flexibleHeight = 0f;
        }

        private void ConfigureCompactLabel(
            TMP_Text text,
            bool flexibleWidth)
        {
            if (text == null)
                return;

            text.enableWordWrapping = false;
            text.overflowMode =
                TextOverflowModes.Ellipsis;
            text.maskable = true;

            if (!flexibleWidth)
                return;

            LayoutElement layout =
                text.GetComponent<LayoutElement>();

            if (layout == null)
            {
                layout =
                    text.gameObject.AddComponent<LayoutElement>();
            }

            layout.minWidth = 0f;
            layout.preferredWidth = 0f;
            layout.flexibleWidth = 1f;
        }

        private void CopyRectTransformGeometry(
            RectTransform source,
            RectTransform target)
        {
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition =
                source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.localScale = Vector3.one;
            target.localRotation = Quaternion.identity;
        }

        private void SetRectWidthKeepingLeft(
            RectTransform rect,
            float targetWidth)
        {
            if (rect == null)
                return;

            float currentWidth =
                rect.rect.width;

            if (currentWidth <= 0.01f)
                currentWidth = targetWidth;

            float delta =
                targetWidth - currentWidth;

            Vector2 size = rect.sizeDelta;
            size.x += delta;
            rect.sizeDelta = size;

            Vector2 position =
                rect.anchoredPosition;

            position.x +=
                rect.pivot.x * delta;

            rect.anchoredPosition = position;
        }

        private void StretchToParent(
            RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private void RebuildSelectorLayout()
        {
            if (_modSelectorContent == null)
                return;

            Canvas.ForceUpdateCanvases();

            RectTransform rect =
                _modSelectorContent.GetComponent<RectTransform>();

            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    rect);
            }

            Canvas.ForceUpdateCanvases();
        }

        private void RebuildLayouts()
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

            if (_modsPanel != null)
            {
                RectTransform rect =
                    _modsPanel.GetComponent<RectTransform>();

                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(
                        rect);
                }
            }

            RebuildSelectorLayout();
            Canvas.ForceUpdateCanvases();
        }

        private void DisableBeamUiBehaviours(
            GameObject root)
        {
            if (root == null)
                return;

            Behaviour[] behaviours =
                root.GetComponentsInChildren<Behaviour>(true);

            foreach (Behaviour behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                Type type = behaviour.GetType();

                if (
                    type.Namespace == null ||
                    !type.Namespace.StartsWith(
                        "Beam.UI",
                        StringComparison.Ordinal)
                )
                {
                    continue;
                }

                behaviour.enabled = false;
                DestroyImmediate(behaviour);
            }
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
                definition == null ||
                definition.Kind == ModSettingKind.Header
            )
            {
                return;
            }

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

                ConfigureWrappedSettingLabel(
                    control,
                    runtime.NameLabel);
            }

            if (runtime.ValueLabel != null)
            {
                ConfigureCompactLabel(
                    runtime.ValueLabel,
                    false);
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
            AttachReturnToSelector(control);

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

                ConfigureWrappedSettingLabel(
                    control,
                    runtime.NameLabel);
            }

            if (runtime.ValueLabel != null)
            {
                ConfigureCompactLabel(
                    runtime.ValueLabel,
                    false);
            }

            button.onClick.AddListener(
                delegate
                {
                    ActivateButtonSetting(
                        runtime);
                });

            AttachAutoScroll(control);
            AttachReturnToSelector(control);

            _runtimeControls.Add(runtime);

            UpdateRuntimeControl(
                runtime);
        }

        private void AttachAutoScroll(
            GameObject control)
        {
            AttachAutoScroll(
                control,
                _scrollRect);
        }

        private void AttachAutoScroll(
            GameObject control,
            ScrollRect scrollRect)
        {
            if (
                control == null ||
                scrollRect == null
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
                scrollRect,
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
            ApplyModsColumnLayout();

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

            RestoreNativeColumnLayout();

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
                RestoreNativeColumnLayout();

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

            RestoreNativeColumnLayout();

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
            RebuildLayouts();

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
