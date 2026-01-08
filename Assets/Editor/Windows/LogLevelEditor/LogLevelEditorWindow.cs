using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using System;
using System.IO;

public class LogLevelEditorWindow : EditorWindow
{
    private static readonly string GLOBAL_LOG_LEVEL_KEY = "GLOBAL_LOG_LEVEL_KEY";

    private ListView listView;
    private EnumField globalLogLevel;
    private List<ProviderItem> items = new(); // Unified list for virtualization
    private readonly Dictionary<string, ScriptableObject> loadedSOs = new();

    [MenuItem("Window/Log Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LogLevelEditorWindow>("Log Level Editor");
    }

    private void CreateGUI()
    {
        LoadUXML();
        LoadUSS();
        InitializeGlobalLogLevel();
        InitializeButtons();
        InitializeListView();
        RefreshProviders();
    }

    private void LoadUXML()
    {
        var uxmlPath = "Assets/Editor/Windows/LogLevelEditor/LogLevelEditor.uxml";
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        if (visualTree == null)
        {
            Debug.LogError("UXML not found at: " + uxmlPath);
            return;
        }
        visualTree.CloneTree(rootVisualElement);
    }

    private void LoadUSS()
    {
        var ussPath = "Assets/Editor/Windows/LogLevelEditor/LogLevelEditor.uss";
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        if (styleSheet != null)
        {
            rootVisualElement.styleSheets.Add(styleSheet);
        }
    }

    private void InitializeGlobalLogLevel()
    {
        globalLogLevel = rootVisualElement.Q<EnumField>();

        if (EditorPrefs.HasKey(GLOBAL_LOG_LEVEL_KEY))
        {
            globalLogLevel.Init((Logging.Level)EditorPrefs.GetInt(GLOBAL_LOG_LEVEL_KEY));
        }
        else if (globalLogLevel.value == null || globalLogLevel.value.GetType() != typeof(Logging.Level))
        {
            globalLogLevel.Init(Logging.DEFAULT_LEVEL);
        }
    }

    private void InitializeButtons()
    {
        var updateAllButton = rootVisualElement.Q<Button>();
        updateAllButton.clicked += UpdateAllLogLevels;

        var refreshButton = rootVisualElement.Q<Button>("RefreshButton");
        refreshButton.clicked += RefreshProviders;
    }

    private void InitializeListView()
    {
        var itemTemplatePath = "Assets/Editor/Windows/LogLevelEditor/LogLevelListItem.uxml";
        var itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemTemplatePath);

        listView = rootVisualElement.Q<ListView>("ItemsList");
        listView.makeItem = () =>
        {
            var element = itemTemplate.Instantiate();
            var logLevels = element.Q<EnumField>("logLevels");
            logLevels.RegisterValueChangedCallback(OnLogLevelEnumChange);
            return element;
        };
        listView.bindItem = BindItem;
        listView.itemsSource = items;
        listView.viewDataKey = "LogLevelEditorListViewKey";
    }

    private void UpdateAllLogLevels()
    {
        EditorPrefs.SetInt(GLOBAL_LOG_LEVEL_KEY, Convert.ToInt16(globalLogLevel.value));

        foreach (var p in items)
        {
            p.UpdateLogLevel((Logging.Level)globalLogLevel.value);
            listView.RefreshItems();
        }
    }

    private void RefreshProviders()
    {
        items.Clear();
        Logging.ClearAllTags();

        // Add scene components
        var components = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
            .Where(c => c is ILoggerProvider)
            .Select(c => new ProviderItem(c));

        // Add ScriptableObjects (lazy load later)
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        var soItems = guids.Select(guid => GetOrLoadSO(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(s => s is ILoggerProvider)
            .Select(s => new ProviderItem(s));

        items.AddRange(components);
        items.AddRange(soItems);
        items = items.OrderBy(p => p.tag.Name).ThenBy(p => p.GetFileExtension()).ThenBy(p => p.tag.TypeName).ToList();

        listView.itemsSource = items;
        listView.RefreshItems();
        listView.itemsChosen -= ItemsChosen;
        listView.itemsChosen += ItemsChosen;
    }

    private void BindItem(VisualElement parent, int index)
    {
        items[index].Bind(parent);
    }

    private ScriptableObject GetOrLoadSO(string path)
    {
        if (loadedSOs.TryGetValue(path, out ScriptableObject so)) return so;
        so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (so != null) loadedSOs[path] = so;
        return so;
    }

    private void OnLogLevelEnumChange(ChangeEvent<Enum> e)
    {
        var p = (ProviderItem)((VisualElement)e.target).userData;
        p.UpdateLogLevel((Logging.Level)e.newValue);
    }

    private void ItemsChosen(IEnumerable<object> items)
    {
        foreach (var item in items)
        {
            var p = (ProviderItem)item;
            p.SelectInEditor();
        }
    }

    // Helper class for items
    private readonly struct ProviderItem
    {
        public readonly Logging.Tag tag;

        private readonly UnityEngine.Object target;
        private readonly Logger logger;
        private readonly string path;

        public ProviderItem(UnityEngine.Object o)
        {
            target = o;
            logger = (o as ILoggerProvider).Logger;
            tag = logger.CreateTag(o);
            logger.SetLogLevel();
            path = AssetDatabase.GetAssetOrScenePath(o);
        }

        public void Bind(VisualElement parent)
        {
            var loadingContainer = parent.Q<VisualElement>("loadingContainer");
            var mainContainer = parent.Q<VisualElement>("mainContainer");
            var displayName = parent.Q<Label>("displayName");
            var logLevels = parent.Q<EnumField>("logLevels");
            var prefabLabel = parent.Q<Label>("prefabLabel");

            displayName.text = tag.Value;
            prefabLabel.style.display = DisplayStyle.Flex;
            prefabLabel.text = path;

            InitLogLevel(logLevels);

            // Force visibility
            loadingContainer.style.display = DisplayStyle.None;
            mainContainer.style.display = DisplayStyle.Flex;
        }

        public string GetFileExtension()
        {
            return Path.GetExtension(path)?.Replace(".", "");
        }

        public void SelectInEditor()
        {
            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        public void UpdateLogLevel(Logging.Level l)
        {
            logger.logLevel = l;
            logger.SetLogLevel();

            if (EditorUtility.IsPersistent(target))
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void InitLogLevel(EnumField logLevels)
        {
            if (logLevels.value == null || logLevels.value.GetType() != typeof(Logging.Level))
            {
                logLevels.Init(Logging.DEFAULT_LEVEL);
            }
            logLevels.userData = this;
            logLevels.SetValueWithoutNotify(logger.logLevel);
        }
    }
}
