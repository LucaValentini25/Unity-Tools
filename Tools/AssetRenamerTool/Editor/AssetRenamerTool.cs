using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using Object = UnityEngine.Object;

namespace com.ClasterTools.AssetRenamer.Editor
{
    public class AssetRenamerTool : EditorWindow
    {
        private List<AssetEntry> assetEntries = new();
        private Dictionary<Object, string> renameHistory = new(); // Object → originalName
        private Vector2 scrollPos;
        private int selectedFilter = 0;
        private int tabIndex = 0;
        private UnityEditor.Editor settingsEditor;

        private static AssetRenamerSettings settings;
        private static string[] filterOptions = new[] { "All" };
        private const string SettingsPrefsKey = "com.clastertools.assetrenamer.settingsPath";

        [MenuItem("Tools/Asset Renamer Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetRenamerTool>("Asset Renamer");
            window.minSize = new Vector2(500, 650);
            LoadSettingsFromPrefs();
            UpdateFilterOptions();
        }

        private static void LoadSettingsFromPrefs()
        {
            string savedPath = EditorPrefs.GetString(SettingsPrefsKey, "");
            if (!string.IsNullOrEmpty(savedPath))
            {
                settings = AssetDatabase.LoadAssetAtPath<AssetRenamerSettings>(savedPath);
            }

            if (settings == null)
            {
                // Intentar buscar uno existente en la carpeta por defecto
                string defaultPath = "Assets/Editor/AssetRenamerSettings/AssetRenamerSettings.asset";
                settings = AssetDatabase.LoadAssetAtPath<AssetRenamerSettings>(defaultPath);

                // Si no existe, crear uno nuevo
                if (settings == null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                        AssetDatabase.CreateFolder("Assets", "Editor");
                    if (!AssetDatabase.IsValidFolder("Assets/Editor/AssetRenamerSettings"))
                        AssetDatabase.CreateFolder("Assets/Editor", "AssetRenamerSettings");

                    settings = ScriptableObject.CreateInstance<AssetRenamerSettings>();
                    AssetDatabase.CreateAsset(settings, defaultPath);
                    AssetDatabase.SaveAssets();
                }

                EditorPrefs.SetString(SettingsPrefsKey, defaultPath);
            }
        }

        private static void UpdateFilterOptions()
        {
            if (settings != null)
            {
                filterOptions = new[] { "All" }.Concat(settings.prefixEntries.Select(e => e.categoryName)).Distinct().ToArray();
            }
        }

        private void OnGUI()
        {
            tabIndex = GUILayout.Toolbar(tabIndex, new[] { "Renamer", "Settings" });
            GUILayout.Space(10);

            switch (tabIndex)
            {
                case 0: DrawRenamerTab(); break;
                case 1: DrawSettingsTab(); break;
            }
        }

        private void DrawRenamerTab()
        {
            if (!settings)
            {
                EditorGUILayout.HelpBox("Settings asset not loaded. Please go to the Settings tab.", MessageType.Error);
                return;
            }

            GUILayout.Label("Drag & Drop Assets Below", EditorStyles.boldLabel);
            Rect dropArea = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop assets here", EditorStyles.helpBox);
            HandleDragAndDrop(dropArea);

            GUILayout.Space(10);
            selectedFilter = EditorGUILayout.Popup("Filter by Category", selectedFilter, filterOptions);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < assetEntries.Count; i++)
            {
                var entry = assetEntries[i];
                if (!PassFilter(entry)) continue;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("Asset", entry.obj, typeof(Object), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    assetEntries.RemoveAt(i);
                    i--;
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                entry.modifyPrefix = EditorGUILayout.Toggle(entry.modifyPrefix, GUILayout.Width(20));
                GUI.enabled = entry.modifyPrefix;
                entry.prefix = EditorGUILayout.TextField("Prefix", entry.prefix);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                entry.modifyName = EditorGUILayout.Toggle(entry.modifyName, GUILayout.Width(20));
                GUI.enabled = entry.modifyName;
                entry.baseName = EditorGUILayout.TextField("Base Name", entry.baseName);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Preview", entry.GetPreviewName());

                if (renameHistory.TryGetValue(entry.obj, out string renamedTo))
                {
                    EditorGUILayout.HelpBox($"Renamed to: {renamedTo}", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply to All")) ApplyRenaming(assetEntries);
            if (GUILayout.Button("Apply to Filtered")) ApplyRenaming(assetEntries.Where(PassFilter).ToList());
            if (GUILayout.Button("Undo")) UndoLastRename();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All")) assetEntries.Clear();
            if (GUILayout.Button("Clear Prefixed"))
            {
                assetEntries.RemoveAll(entry =>
                {
                    string currentName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(entry.obj));
                    return currentName.StartsWith(entry.prefix + "_");
                });
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettingsTab()
        {
            GUILayout.Label("Active Settings", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Settings Asset", settings, typeof(AssetRenamerSettings), false);
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create New"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Create Settings", "AssetRenamerSettings", "asset", "Save settings asset");
                if (!string.IsNullOrEmpty(path))
                {
                    var newSettings = CreateInstance<AssetRenamerSettings>();
                    AssetDatabase.CreateAsset(newSettings, path);
                    AssetDatabase.SaveAssets();
                    settings = newSettings;
                    EditorPrefs.SetString(SettingsPrefsKey, path);
                    UpdateFilterOptions();
                }
            }

            if (GUILayout.Button("Import"))
            {
                string path = EditorUtility.OpenFilePanel("Import Settings", Application.dataPath, "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    path = FileUtil.GetProjectRelativePath(path);
                    var imported = AssetDatabase.LoadAssetAtPath<AssetRenamerSettings>(path);
                    if (imported != null)
                    {
                        settings = imported;
                        EditorPrefs.SetString(SettingsPrefsKey, path);
                        UpdateFilterOptions();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid File", "That file is not a valid AssetRenamerSettings.", "OK");
                    }
                }
            }

            if (GUILayout.Button("Export"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Export Settings", "AssetRenamerSettings_Export", "asset", "Choose where to save the export");
                if (!string.IsNullOrEmpty(path) && settings != null)
                {
                    var duplicate = Instantiate(settings);
                    AssetDatabase.CreateAsset(duplicate, path);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Exported", "Settings successfully exported!", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();

            if (settings == null)
            {
                EditorGUILayout.HelpBox("No settings loaded.", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);
            GUILayout.Label("Prefix Settings", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < settings.prefixEntries.Count; i++)
            {
                var entry = settings.prefixEntries[i];

                EditorGUILayout.BeginVertical("box");
                entry.categoryName = EditorGUILayout.TextField("Category", entry.categoryName);
                entry.extension = EditorGUILayout.TextField("Extension", entry.extension);
                entry.prefix = EditorGUILayout.TextField("Prefix", entry.prefix);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove"))
                {
                    settings.prefixEntries.RemoveAt(i);
                    i--;
                    EditorUtility.SetDirty(settings);
                    UpdateFilterOptions();
                    continue;
                }

                if (GUILayout.Button("Duplicate"))
                {
                    settings.prefixEntries.Insert(i + 1, new PrefixEntry
                    {
                        categoryName = entry.categoryName + "_Copy",
                        extension = entry.extension,
                        prefix = entry.prefix
                    });
                    EditorUtility.SetDirty(settings);
                    UpdateFilterOptions();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add New Category"))
            {
                settings.prefixEntries.Add(new PrefixEntry
                {
                    categoryName = "NewCategory",
                    extension = ".ext",
                    prefix = "new_"
                });
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(settings);
                UpdateFilterOptions();
            }
        }

        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var path in DragAndDrop.paths)
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (obj != null)
                        {
                            string extension = Path.GetExtension(path).ToLower();
                            string category = "";
                            string customPrefix = settings != null ? settings.GetPrefixForExtension(extension, out category) : null;
                            assetEntries.Add(new AssetEntry(obj, customPrefix, category));
                        }
                    }
                }
                evt.Use();
            }
        }

        private bool PassFilter(AssetEntry entry)
        {
            if (selectedFilter == 0) return true;
            return entry.categoryName == filterOptions[selectedFilter];
        }

        private void ApplyRenaming(List<AssetEntry> entries)
        {
            foreach (var entry in entries)
            {
                string path = AssetDatabase.GetAssetPath(entry.obj);
                string folder = Path.GetDirectoryName(path);
                string newName = entry.GetPreviewName();
                string newPath = Path.Combine(folder, newName + Path.GetExtension(path));

                if (AssetDatabase.LoadAssetAtPath<Object>(newPath) == null)
                {
                    string originalName = Path.GetFileNameWithoutExtension(path);
                    renameHistory[entry.obj] = originalName;
                    AssetDatabase.RenameAsset(path, newName);
                }
                else
                {
                    Debug.LogWarning($"Duplicate name detected: {newName}");
                }
            }

            AssetDatabase.SaveAssets();
        }


        private void UndoLastRename()
        {
            foreach (var kvp in renameHistory)
            {
                string currentPath = AssetDatabase.GetAssetPath(kvp.Key);
                string extension = Path.GetExtension(currentPath);
                string folder = Path.GetDirectoryName(currentPath);
                string undoPath = Path.Combine(folder, kvp.Value + extension);

                if (AssetDatabase.LoadAssetAtPath<Object>(undoPath) == null)
                {
                    AssetDatabase.RenameAsset(currentPath, kvp.Value);
                }
                else
                {
                    Debug.LogWarning($"Cannot undo rename for {kvp.Key.name} → target name already exists.");
                }
            }

            renameHistory.Clear();
            AssetDatabase.SaveAssets();
            Debug.Log("Undo complete.");
        }

        private class AssetEntry
        {
            public Object obj;
            public string prefix;
            public string baseName;
            public bool modifyPrefix = false;
            public bool modifyName = false;
            public string categoryName;

            public AssetEntry(Object obj, string overridePrefix, string categoryName)
            {
                this.obj = obj;
                string path = AssetDatabase.GetAssetPath(obj);
                string name = Path.GetFileNameWithoutExtension(path);

                this.categoryName = categoryName ?? "Unknown";
                this.prefix = overridePrefix ?? "file";
                this.baseName = name.Replace(" ", "_");
            }

            public string GetPreviewName()
            {
                string cleanedName = Regex.Replace(baseName, "[^a-zA-Z0-9_]+", "").Replace(" ", "_");
                return $"{prefix}_{cleanedName}";
            }
        }
    }
}
