// File: Assets/Editor/TmxImporterSettingsProvider.cs
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

static class TmxImporterSettingsProvider
{
    private const string k_Path = "Project/Tmx Importer";

    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new SettingsProvider(k_Path, SettingsScope.Project)
        {
            label = "Tmx Importer",

            activateHandler = (searchContext, rootElement) =>
            {
                var settings = GetOrCreateSettings();

                // ────────────────────────────────────────────────
                // Container & title
                // ────────────────────────────────────────────────
                var container = new VisualElement
                {
                    style =
                    {
                        paddingTop = 12,
                        paddingLeft = 12,
                        paddingRight = 12,
                        paddingBottom = 12
                    }
                };
                rootElement.Add(container);

                var title = new Label("Tmx Importer Settings")
                {
                    style =
                    {
                        fontSize = 16,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginBottom = 8
                    }
                };
                container.Add(title);

                var description = new Label("Tilemaps imported from Tiled (ST2U). Edit sorting/layer here.")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Italic,
                        color = new Color(0.7f, 0.7f, 0.7f),
                        marginBottom = 16
                    }
                };
                container.Add(description);

                // ────────────────────────────────────────────────
                // Reorderable list of tilemap settings
                // ────────────────────────────────────────────────
                var listView = new ListView
                {
                    itemsSource = settings.tilemapSettingsList,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    showBoundCollectionSize = false,
                    showFoldoutHeader = true,
                    headerTitle = "Imported Tilemaps",
                    style = { minHeight = 240, flexGrow = 1 },
                    makeItem = () =>
                        {
                            var mainContent = new VisualElement
                            {
                                style =
                                {
                                    flexDirection = FlexDirection.Column,
                                    alignItems = Align.FlexStart,
                                    marginBottom = 4,
                                    paddingTop = 2,
                                    paddingBottom = 2,
                                    paddingLeft = 4,
                                    paddingRight = 4,
                                    backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.2f)
                                }
                            };
                            var topRow = new VisualElement
                            {
                                style =
                                {
                                    flexDirection = FlexDirection.Row,
                                    alignItems = Align.Center,
                                    marginBottom = 4,
                                    paddingTop = 2,
                                    paddingBottom = 2,
                                    paddingLeft = 4,
                                    paddingRight = 4,
                                }
                            };
                            var bottomRow = new VisualElement
                            {
                                style =
                                {
                                    flexDirection = FlexDirection.Row,
                                    alignItems = Align.Center,
                                    marginBottom = 4,
                                    paddingTop = 2,
                                    paddingBottom = 2,
                                    paddingLeft = 4,
                                    paddingRight = 4,
                                }
                            };
                            var foldout = new Foldout()
                            {
                                name = "Foldout",
                                style =
                                {
                                    marginLeft = 16,
                                    minHeight = 32,
                                }
                            };
                            var layerLabel = new Label
                            {
                                text = "Layer",
                            };
                            var layerField = new DropdownField()
                            {
                                style = {
                                    minWidth = 50,
                                    marginLeft = 0
                                },
                                choices = InternalEditorUtility.layers.ToList(),
                            };
                            var orderLabel = new Label
                            {
                                text = "Sorting Order",
                            };
                            var orderField = new IntegerField()
                            {
                                style = { minWidth = 120, marginLeft = 0 }
                            };
                            var removeButton = new Button
                            {
                                text = "Remove",
                            };
                            topRow.Add(layerLabel);
                            topRow.Add(layerField);
                            bottomRow.Add(orderLabel);
                            bottomRow.Add(orderField);
                            mainContent.Add(topRow);
                            mainContent.Add(bottomRow);
                            mainContent.Add(removeButton);
                            foldout.contentContainer.Add(mainContent);

                            return foldout;
                        },
                };

                listView.bindItem = (element, index) =>
                {
                    var item = settings.tilemapSettingsList[index];
                    var foldout = element.Q<Foldout>();
                    var layerField = element.Q<DropdownField>();
                    var orderField = element.Q<IntegerField>();
                    var removeButton = element.Q<Button>();

                    foldout.text = string.IsNullOrEmpty(item.name) ? $"Tilemap {index + 1}" : item.name;

                    var layerIndex = item?.layer ?? 0;
                    layerField.value = LayerMask.LayerToName(layerIndex);
                    orderField.value = item?.sortingOrder ?? 0;

                    // Save on change
                    layerField.RegisterValueChangedCallback(evt =>
                    {
                        item.layer = LayerMask.NameToLayer(evt.newValue);
                        SaveSettings(settings);
                    });

                    orderField.RegisterValueChangedCallback(evt =>
                    {
                        item.sortingOrder = evt.newValue;
                        SaveSettings(settings);
                    });
                    removeButton.clicked += () =>
                    {
                        listView.itemsSource.RemoveAt(index);
                        listView.Rebuild();
                    };
                };

                container.Add(listView);

                // ────────────────────────────────────────────────
                // Buttons
                // ────────────────────────────────────────────────
                var buttonRow = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, marginTop = 16 }
                };

                var refreshBtn = new Button()
                {
                    text = "Refresh List (after import)",
                    style = { flexGrow = 1, marginRight = 8 }
                };
                refreshBtn.clicked += () => listView.Rebuild();
                buttonRow.Add(refreshBtn);

                var saveBtn = new Button()
                {
                    text = "Save Changes",
                    style = { flexGrow = 1 }
                };
                saveBtn.clicked += () => SaveSettings(settings);
                buttonRow.Add(saveBtn);

                container.Add(buttonRow);
            }
        };
    }

    private static TmxImporterSettings GetOrCreateSettings()
    {
        const string path = "Assets/Settings/TmxImporterSettings.asset";

        var asset = AssetDatabase.LoadAssetAtPath<TmxImporterSettings>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<TmxImporterSettings>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        return asset;
    }

    private static void SaveSettings(TmxImporterSettings settings)
    {
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssetIfDirty(settings);
    }
}