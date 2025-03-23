using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using Object = UnityEngine.Object;

public class AssetRenamerTool : EditorWindow
{
    private List<AssetEntry> assetEntries = new();
    private Vector2 scrollPos;
    private int selectedFilter = 0;
    private int tabIndex = 0;
    private Editor settingsEditor;

    private static AssetRenamerSettings settings;
    private static string[] filterOptions = new[] { "All" };
    private const string SettingsPrefsKey = "AssetRenamerSettingsPath";

    [MenuItem("Tools/Asset Renamer Tool")]
    public static void ShowWindow()
    {
        GetWindow<AssetRenamerTool>("Asset Renamer");
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
            var guids = AssetDatabase.FindAssets("t:AssetRenamerSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<AssetRenamerSettings>(path);
                EditorPrefs.SetString(SettingsPrefsKey, path);
            }
            else
            {
                settings = CreateInstance<AssetRenamerSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Settings/AssetRenamerSettings.asset");
                AssetDatabase.SaveAssets();
                EditorPrefs.SetString(SettingsPrefsKey, "Assets/Settings/AssetRenamerSettings.asset");
            }
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
            case 0:
                DrawRenamerTab();
                break;
            case 1:
                DrawSettingsTab();
                break;
        }
    }

    private void DrawRenamerTab()
    {
        if (!settings)
        {
            EditorGUILayout.HelpBox("Settings asset not loaded, please go to Setting tab", MessageType.Error);
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

    // Mostrar el asset actual (readonly)
    GUI.enabled = false;
    EditorGUILayout.ObjectField("Settings Asset", settings, typeof(AssetRenamerSettings), false);
    GUI.enabled = true;

    // Botones de gestión
    EditorGUILayout.BeginHorizontal();
    if (GUILayout.Button("Create New Settings Asset"))
    {
        string path = EditorUtility.SaveFilePanelInProject("Create AssetRenamerSettings", "AssetRenamerSettings", "asset", "Choose a location to save the settings");
        if (!string.IsNullOrEmpty(path))
        {
            var newSettingsAsset = CreateInstance<AssetRenamerSettings>();
            AssetDatabase.CreateAsset(newSettingsAsset, path);
            AssetDatabase.SaveAssets();
            settings = newSettingsAsset;
            EditorPrefs.SetString(SettingsPrefsKey, path);
            UpdateFilterOptions();
            settingsEditor = null;
        }
    }

    if (GUILayout.Button("Select Settings Asset"))
    {
        string path = EditorUtility.OpenFilePanel("Select AssetRenamerSettings", Application.dataPath, "asset");
        if (!string.IsNullOrEmpty(path))
        {
            path = FileUtil.GetProjectRelativePath(path);
            var loadedSettings = AssetDatabase.LoadAssetAtPath<AssetRenamerSettings>(path);
            if (loadedSettings != null)
            {
                settings = loadedSettings;
                EditorPrefs.SetString(SettingsPrefsKey, path);
                UpdateFilterOptions();
                settingsEditor = null;
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Selection", "The selected file is not a valid AssetRenamerSettings asset.", "OK");
            }
        }
    }
    EditorGUILayout.EndHorizontal();
        
    if (settings == null)
    {
        EditorGUILayout.HelpBox("Settings not loaded.", MessageType.Warning);
        EditorGUILayout.EndScrollView();
        return;
    }
    
    EditorGUILayout.Space(10);
    EditorGUILayout.LabelField("Prefix Settings", EditorStyles.boldLabel);
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
                        string category = "Unknown";
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
            string newPath = folder + "/" + newName + Path.GetExtension(path);

            if (AssetDatabase.LoadAssetAtPath<Object>(newPath) == null)
            {
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
        Debug.Log("Undo not implemented yet.");
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

[System.Serializable]
public class PrefixEntry
{
    public string categoryName;
    public string extension;
    public string prefix;
}

public class AssetRenamerSettings : ScriptableObject
{
    public List<PrefixEntry> prefixEntries = new List<PrefixEntry>()
    { 
        new PrefixEntry { categoryName = "Animation",         extension = ".anim",  prefix = "Anim" },
        new PrefixEntry { categoryName = "Animator Controller", extension = ".controller", prefix = "AnimCon" },
        new PrefixEntry { categoryName = "Audio",             extension = ".wav",   prefix = "Sound" },
        new PrefixEntry { categoryName = "Audio",             extension = ".mp3",   prefix = "Sound" },
        new PrefixEntry { categoryName = "Audio",             extension = ".ogg",   prefix = "Sound" },
        new PrefixEntry { categoryName = "Material",          extension = ".mat",   prefix = "Mat" },
        new PrefixEntry { categoryName = "Mesh",              extension = ".fbx",   prefix = "Mesh" },
        new PrefixEntry { categoryName = "Mesh",              extension = ".obj",   prefix = "Mesh" },
        new PrefixEntry { categoryName = "Prefab",            extension = ".prefab",prefix = "Prefab" },
        new PrefixEntry { categoryName = "Shader",            extension = ".shader",prefix = "Shad" },
        new PrefixEntry { categoryName = "Shader Graph",      extension = ".shadergraph", prefix = "SH_Graph" },
        new PrefixEntry { categoryName = "Texture",           extension = ".png",   prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".jpg",   prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".jpeg",  prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".tga",   prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".psd",   prefix = "Tex" },
        new PrefixEntry { categoryName = "ScriptableObject",  extension = ".asset", prefix = "SO" },
        new PrefixEntry { categoryName = "Timeline",          extension = ".playable", prefix = "Timeline" },
        new PrefixEntry { categoryName = "Sprite Atlas",      extension = ".spriteatlas", prefix = "Sprite_Atl" },
        new PrefixEntry { categoryName = "Lighting Settings", extension = ".lighting", prefix = "Light" },
        new PrefixEntry { categoryName = "Render Texture",    extension = ".renderTexture", prefix = "Rend_Tex" },
    };

    public string GetPrefixForExtension(string ext, out string categoryName)
    {
        foreach (var entry in prefixEntries)
        {
            if (entry.extension == ext)
            {
                categoryName = entry.categoryName;
                return entry.prefix;
            }
        }
        categoryName = "Unknown";
        return null;
    }
}