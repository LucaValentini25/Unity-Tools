#if UNITY_EDITOR_WIN

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
namespace com.ClasterTools.PlayerPrefsManager.Editor
{
public class PlayerPrefsManager : EditorWindow
{
    private Vector2 scrollPos;
    private List<PlayerPrefEntry> prefs = new();
    private List<PlayerPrefEntry> newPrefs = new();
    private int tabIndex = 0;
    private Dictionary<string, float> copiedKeys = new();
    private Dictionary<string, float> copiedValues = new();
    private HashSet<string> lockedKeys = new();

    private static readonly string[] unityInternalKeys =
    {
        "unity.cloud_userid",
        "unity.player_sessionid",
        "unity.player_session_count"
    };

    [MenuItem("Tools/PlayerPrefs Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayerPrefsManager>("PlayerPrefs Manager");
        window.minSize = new Vector2(600, 1000);
        window.ReloadPrefs();
    }

    private void ReloadPrefs()
    {
        prefs = PlayerPrefsRegistryViewer.FetchPrefs()
            .Where(p => !unityInternalKeys.Contains(p.key))
            .ToList();
    }

    private void OnGUI()
    {
        tabIndex = GUILayout.Toolbar(tabIndex, new[] { "[ Existing Prefs ]", "[ Add New ]" });
        GUILayout.Space(10);

        switch (tabIndex)
        {
            case 0:
                DrawExistingPrefs();
                break;
            case 1:
                DrawAddNewPref();
                break;
        }
    }

    private void DrawExistingPrefs()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Existing PlayerPrefs", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reload", GUILayout.Width(80))) ReloadPrefs();
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var entry in prefs)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            // Content area (75%)
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.75f));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key:", entry.key);
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = lockedKeys.Contains(entry.key) ? Color.red : Color.white;
            if (GUILayout.Button(lockedKeys.Contains(entry.key) ? "Locked" : "Unlocked", GUILayout.Width(80)))
            {
                if (lockedKeys.Contains(entry.key)) lockedKeys.Remove(entry.key);
                else lockedKeys.Add(entry.key);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Type:", entry.type.ToString());
            entry.value = EditorGUILayout.TextField("Value", entry.value);
            EditorGUILayout.EndVertical();

            GUILayout.Space(position.width * 0.05f); // 5% gap

            // Button area (15%)
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.15f));

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Save")) SavePref(entry);

            GUI.backgroundColor = Color.red;
            GUI.enabled = !lockedKeys.Contains(entry.key);
            if (GUILayout.Button("Delete")) DeletePref(entry.key);
            GUI.enabled = true;

            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button(copiedKeys.TryGetValue(entry.key, out float t1) && Time.realtimeSinceStartup - t1 < 1f ? "Copied" : "Copy Key"))
            {
                EditorGUIUtility.systemCopyBuffer = entry.key;
                copiedKeys[entry.key] = Time.realtimeSinceStartup;
                Repaint();
            }

            if (GUILayout.Button(copiedValues.TryGetValue(entry.key, out float t2) && Time.realtimeSinceStartup - t2 < 1f ? "Copied" : "Copy Value"))
            {
                EditorGUIUtility.systemCopyBuffer = entry.value;
                copiedValues[entry.key] = Time.realtimeSinceStartup;
                Repaint();
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
        if (GUILayout.Button("Clear All PlayerPrefs"))
        {
            if (EditorUtility.DisplayDialog("Confirm Clear", "Are you sure you want to delete ALL PlayerPrefs (except locked)?", "Yes", "Cancel"))
            {
                foreach (var entry in prefs)
                {
                    if (!lockedKeys.Contains(entry.key))
                    {
                        PlayerPrefs.DeleteKey(entry.key);
                    }
                }
                PlayerPrefs.Save();
                Debug.Log("Non-locked PlayerPrefs cleared.");
                ReloadPrefs();
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawAddNewPref()
    {
        float scrollWidth = Mathf.Max(position.width,400);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Width(scrollWidth));

        foreach (var newPref in newPrefs)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth * 0.95f));

            float height = EditorGUIUtility.singleLineHeight + 4;
            float removeBtnSize = height;
            float spacing = 6f;
            float fieldWidth = scrollWidth - spacing * 3;
            fieldWidth *= 0.95f;
            GUILayout.Label("Key", GUILayout.Width(50));
            newPref.key = EditorGUILayout.TextField(newPref.key, GUILayout.Width((fieldWidth * 0.9f) - removeBtnSize));
            GUILayout.Space(spacing/2);

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(removeBtnSize)))
            {
                newPrefs.Remove(newPref);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Value", GUILayout.Width(50));
            newPref.value = EditorGUILayout.TextField(newPref.value, GUILayout.Width((fieldWidth * 0.7f)-spacing));
            GUILayout.Space(spacing/2);
            newPref.type = (PlayerPrefType)EditorGUILayout.EnumPopup(newPref.type, GUILayout.Width(fieldWidth * 0.2f));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add New PlayerPref"))
        {
            newPrefs.Add(new PlayerPrefEntry());
        }

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Apply New Prefs")) ApplyNewPrefs();
        GUI.backgroundColor = Color.white;
    }

    private void SavePref(PlayerPrefEntry entry)
    {
        if (!Validate(entry)) return;

        switch (entry.type)
        {
            case PlayerPrefType.Int:
                if (int.TryParse(entry.value, out int intVal))
                    PlayerPrefs.SetInt(entry.key, intVal);
                break;
            case PlayerPrefType.Float:
                if (float.TryParse(entry.value, out float floatVal))
                    PlayerPrefs.SetFloat(entry.key, floatVal);
                break;
            case PlayerPrefType.String:
                PlayerPrefs.SetString(entry.key, entry.value);
                break;
        }

        PlayerPrefs.Save();
    }

    private void DeletePref(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        ReloadPrefs();
    }

    private void ApplyNewPrefs()
    {
        foreach (var newEntry in newPrefs)
        {
            if (!Validate(newEntry) || PlayerPrefs.HasKey(newEntry.key)) continue;

            switch (newEntry.type)
            {
                case PlayerPrefType.Int:
                    if (int.TryParse(newEntry.value, out int intVal))
                        PlayerPrefs.SetInt(newEntry.key, intVal);
                    break;
                case PlayerPrefType.Float:
                    if (float.TryParse(newEntry.value, out float floatVal))
                        PlayerPrefs.SetFloat(newEntry.key, floatVal);
                    break;
                case PlayerPrefType.String:
                    PlayerPrefs.SetString(newEntry.key, newEntry.value);
                    break;
            }
        }

        PlayerPrefs.Save();
        newPrefs.Clear();
        ReloadPrefs();
    }

    private bool Validate(PlayerPrefEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.key))
        {
            Debug.LogWarning("PlayerPref key is empty.");
            return false;
        }
        return true;
    }
}

public enum PlayerPrefType { Int, Float, String }

public class PlayerPrefEntry
{
    public string key = "";
    public string value = "";
    public PlayerPrefType type = PlayerPrefType.String;
}
}
#endif
