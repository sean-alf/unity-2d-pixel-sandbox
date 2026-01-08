using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using System;

public class LogLevelEditorWindow : EditorWindow
{
    private struct LogLevelCallbackData
    {
        public UnityEngine.Object target;
        public ILoggerProvider provider;
    }

    private ListView listView;
    private List<ProviderItem> items = new(); // Unified list for virtualization
    private readonly Dictionary<string, ScriptableObject> loadedSOs = new();

    [MenuItem("Window/Log Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LogLevelEditorWindow>("Log Level Editor");
    }

    private void CreateGUI()
    {
        rootVisualElement.style.flexGrow = 1;

        var uxmlPath = "Assets/Editor/Windows/LogLevelEditor/LogLevelEditor.uxml";
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        if (visualTree == null)
        {
            Debug.LogError("UXML not found at: " + uxmlPath);
            return;
        }
        visualTree.CloneTree(rootVisualElement);

        var ussPath = "Assets/Editor/Windows/LogLevelEditor/LogLevelEditor.uss";
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        if (styleSheet != null)
        {
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        var itemTemplatePath = "Assets/Editor/Windows/LogLevelEditor/LogLevelListItem.uxml";
        var itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemTemplatePath);

        if (itemTemplate == null)
        {
            Debug.LogError("Item template UXML not found at: " + itemTemplatePath);
            return;
        }

        var refreshButton = rootVisualElement.Q<Button>("RefreshButton");
        refreshButton.clicked += RefreshProviders;

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

        RefreshProviders(); // Initial data load
    }

    private void RefreshProviders()
    {
        items.Clear();

        // Add scene components
        var components = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
            .Where(c => c is ILoggerProvider)
            .Select(c => new ProviderItem
            {
                isPrefab = EditorUtility.IsPersistent(c.gameObject),
                component = c,
                displayName = $"{c.name} ({c.GetType().Name})"
            });

        // Add ScriptableObjects (lazy load later)
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        var soItems = guids.Select(guid => GetOrLoadSO(AssetDatabase.GUIDToAssetPath(guid))).Where(s => s is ILoggerProvider).Select(s => new ProviderItem
        {
            isPrefab = false,
            component = s,
            displayName = $"{s.name} (Asset)",
        });

        items.AddRange(components);
        items.AddRange(soItems);
        items = items.OrderBy(p => p.displayName).ToList();

        listView.itemsSource = items;
        listView.RefreshItems();

        listView.itemsChosen -= ItemsChosen;
        listView.itemsChosen += ItemsChosen;
    }

    private void BindItem(VisualElement element, int index)
    {
        var data = items[index];
        var loadingContainer = element.Q<VisualElement>("loadingContainer");
        var mainContainer = element.Q<VisualElement>("mainContainer");
        var displayName = element.Q<Label>("displayName");
        var logLevels = element.Q<EnumField>("logLevels");
        var prefabLabel = element.Q<Label>("prefabLabel");

        ILoggerProvider provider;
        UnityEngine.Object target;

        provider = data.component as ILoggerProvider;
        target = data.component;
        displayName.text = data.displayName;

        if (data.isPrefab)
        {
            prefabLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            prefabLabel.style.display = DisplayStyle.None;
        }

        if (logLevels.value == null || logLevels.value.GetType() != typeof(Logging.Level))
        {
            logLevels.Init(Logging.Level.INFO);
        }

        logLevels.userData = new LogLevelCallbackData()
        {
            target = target,
            provider = provider,
        };
        logLevels.SetValueWithoutNotify(provider.Logger.logLevel);

        // Force visibility
        loadingContainer.style.display = DisplayStyle.None;
        mainContainer.style.display = DisplayStyle.Flex;
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
        var data = (LogLevelCallbackData)((VisualElement)e.target).userData;

        data.provider.Logger.logLevel = (Logging.Level)e.newValue;
        data.provider.Logger.SetLogLevel();

        if (EditorUtility.IsPersistent(data.target))
        {
            EditorUtility.SetDirty(data.target);
        }
    }

    private void ItemsChosen(IEnumerable<object> items)
    {
        foreach (var item in items)
        {
            var p = (ProviderItem)item;
            Selection.activeObject = p.component;
            EditorGUIUtility.PingObject(p.component);
        }
    }

    // Helper class for items
    private struct ProviderItem
    {
        public bool isPrefab;
        public UnityEngine.Object component;
        public string displayName;
    }
}
