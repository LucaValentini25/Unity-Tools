using UnityEngine;
using UnityEditor;
using System.Linq;

namespace com.ClasterTools.RenameItems.Editor
{
public class RenameItems : EditorWindow
{
    private string _baseName = "Item";
    private bool _useCustomBaseName = false;
    private bool _enumerate = false;
    private int _startIndex = 1;
    private EnumerationFormat _format = EnumerationFormat.Item1;
    public GameObject[] objectsToRename;
    private Vector2 _mainScrollPos;
    private Vector2 _previewScrollPos;
    private Vector2 _objectsScrollPos;

    public enum EnumerationFormat
    {
        Item1,
        Item_1,
        ItemParentheses1
    }

    [MenuItem("Tools/Rename Items")]
    public static void ShowWindow()
    {
        RenameItems window = GetWindow<RenameItems>("Rename Items");
        window.minSize = new Vector2(400, 650);
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Rename a batch of GameObjects with optional enumeration formats. Useful for organizing objects quickly.",
            MessageType.Info);

        _mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos);

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty objectsProp = serializedObject.FindProperty("objectsToRename");

        EditorGUILayout.LabelField("Objects to Rename:", EditorStyles.boldLabel);
        _objectsScrollPos = EditorGUILayout.BeginScrollView(_objectsScrollPos, GUILayout.Height(150));
        EditorGUILayout.PropertyField(objectsProp, GUIContent.none, true);
        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Selected Objects")) AddSelectedObjects();
        if (GUILayout.Button("Clear Objects")) ClearObjects();
        EditorGUILayout.EndHorizontal();

        _useCustomBaseName = EditorGUILayout.Toggle("Use Custom Base Name", _useCustomBaseName);

        if (objectsToRename != null && objectsToRename.Length > 0 && 
            !string.IsNullOrEmpty(objectsToRename[0]?.name) && !_useCustomBaseName)
        {
            _baseName = objectsToRename[0].name;
        }

        EditorGUI.BeginDisabledGroup(!_useCustomBaseName);
        _baseName = EditorGUILayout.TextField("Base Name", _baseName);
        EditorGUI.EndDisabledGroup();

        _enumerate = EditorGUILayout.Toggle("Enumerate", _enumerate);

        if (_enumerate)
        {
            _format = (EnumerationFormat)EditorGUILayout.EnumPopup("Format", _format);
            _startIndex = EditorGUILayout.IntField("Start From", _startIndex);
            if (_startIndex < 0) _startIndex = 0;
        }

        if (objectsToRename != null && objectsToRename.Length > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Preview of Names:", EditorStyles.boldLabel);
            _previewScrollPos = EditorGUILayout.BeginScrollView(_previewScrollPos, GUILayout.Height(150));
            for (int i = 0; i < objectsToRename.Length; i++)
            {
                if (objectsToRename[i] != null)
                {
                    string preview = GetFormattedName(i);
                    EditorGUILayout.LabelField($"• {objectsToRename[i].name} → {preview}");
                }
            }
            EditorGUILayout.EndScrollView();
        }

        if (GUILayout.Button("Rename Objects"))
        {
            RenameSelectedObjects();
        }

        EditorGUILayout.EndScrollView();
    }

    private string GetFormattedName(int index)
    {
        string name = _baseName;
        int number = _startIndex + index;

        if (_enumerate)
        {
            switch (_format)
            {
                case EnumerationFormat.Item1:
                    return name + " " + number;
                case EnumerationFormat.Item_1:
                    return name + "_" + number;
                case EnumerationFormat.ItemParentheses1:
                    return name + " (" + number + ")";
            }
        }
        return name;
    }

    private void RenameSelectedObjects()
    {
        if (objectsToRename == null || objectsToRename.Length == 0)
        {
            Debug.LogWarning("No objects selected for renaming.");
            return;
        }

        objectsToRename = objectsToRename
            .Where(obj => obj != null)
            .Distinct()
            .ToArray();

        for (int i = 0; i < objectsToRename.Length; i++)
        {
            if (objectsToRename[i] != null)
            {
                Undo.RecordObject(objectsToRename[i], "Rename Objects");
                objectsToRename[i].name = GetFormattedName(i);
            }
        }

        Debug.Log("Objects renamed successfully.");
    }

    private void ClearObjects()
    {
        objectsToRename = new GameObject[0];
        Debug.Log("Array cleared.");
    }

    private void AddSelectedObjects()
    {
        objectsToRename = Selection.gameObjects;
        Debug.Log($"{objectsToRename.Length} objects added to the array.");
    }
}

}