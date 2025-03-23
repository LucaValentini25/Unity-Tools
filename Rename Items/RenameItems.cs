using UnityEngine;
using UnityEditor;

public class RenameItems : EditorWindow
{
    private string _baseName = "Item";
    private bool _useCustomBaseName = false; 
    private bool _enumerate = false;
    private EnumerationFormat _format = EnumerationFormat.Item1;
    public GameObject[] objectsToRename; 
    private string _previewName = ""; 

    private Vector2 _scrollPos;

    // Enum to define enumeration formats
    public enum EnumerationFormat
    {
        Item1,            // Item 1
        Item_1,           // Item_1
        ItemParentheses1  // Item (1)
    }

    [MenuItem("Tools/Rename Items")]
    public static void ShowWindow()
    {
        RenameItems window = GetWindow<RenameItems>("Rename Items");
        window.minSize = new Vector2(300, 400); // Minimum size
    }

    private void OnGUI()
    {
        // Scroll view for the content
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        // Display the array of objects for manual assignment
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty objectsProp = serializedObject.FindProperty("objectsToRename");

        // Dropdown to select objects
        EditorGUILayout.PropertyField(objectsProp, new GUIContent("Objects to Rename"), true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginHorizontal();
        // Button to add selected objects from the hierarchy to the array
        if (GUILayout.Button("Add Selected Objects"))
        {
            AddSelectedObjects();
        }

        // Button to clear the array
        if (GUILayout.Button("Clear Objects"))
        {
            ClearObjects();
        }
        EditorGUILayout.EndHorizontal();

        // Bool to decide if the Base Name is custom or not
        _useCustomBaseName = EditorGUILayout.Toggle("Use Custom Base Name", _useCustomBaseName);

        // Update baseName only if it is not custom and there is a first object
        if (objectsToRename != null && objectsToRename.Length > 0 && 
            !string.IsNullOrEmpty(objectsToRename[0]?.name) && !_useCustomBaseName)
        {
            _baseName = objectsToRename[0].name;
        }

        // Display the Base Name as readonly if it is not custom
        EditorGUI.BeginDisabledGroup(!_useCustomBaseName); // Disables the field if not custom
        _baseName = EditorGUILayout.TextField("Base Name", _baseName);
        EditorGUI.EndDisabledGroup();

        // Checkbox to enumerate objects
        _enumerate = EditorGUILayout.Toggle("Enumerate", _enumerate);

        // If enumeration is enabled, display format options
        if (_enumerate)
        {
            _format = (EnumerationFormat)EditorGUILayout.EnumPopup("Format", _format);

            // Update the preview name based on the selected format
            UpdatePreviewName();

            // Display a readonly field with the previewed name
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Preview", _previewName);
            EditorGUI.EndDisabledGroup();
        }

        // Button to execute renaming
        if (GUILayout.Button("Rename Objects"))
        {
            RenameSelectedObjects();
        }

        EditorGUILayout.EndScrollView(); // End of scroll view
    }

    // Method to update the preview name based on the selected format
    private void UpdatePreviewName()
    {
        _previewName = _baseName; // Start with the baseName

        if (_enumerate)
        {
            // Simulate the name of the first enumerated object (1)
            switch (_format)
            {
                case EnumerationFormat.Item1:
                    _previewName += " 1";
                    break;
                case EnumerationFormat.Item_1:
                    _previewName += "_1";
                    break;
                case EnumerationFormat.ItemParentheses1:
                    _previewName += " (1)";
                    break;
            }
        }
    }

    private void RenameSelectedObjects()
    {
        if (objectsToRename == null || objectsToRename.Length == 0)
        {
            Debug.LogWarning("No objects selected for renaming.");
            return;
        }

        for (int i = 0; i < objectsToRename.Length; i++)
        {
            if (objectsToRename[i] != null) // Ensure the object is not null
            {
                string newName = _baseName;

                if (_enumerate)
                {
                    switch (_format)
                    {
                        case EnumerationFormat.Item1:
                            newName += " " + (i + 1);
                            break;
                        case EnumerationFormat.Item_1:
                            newName += "_" + (i + 1);
                            break;
                        case EnumerationFormat.ItemParentheses1:
                            newName += " (" + (i + 1) + ")";
                            break;
                    }
                }

                objectsToRename[i].name = newName;
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
