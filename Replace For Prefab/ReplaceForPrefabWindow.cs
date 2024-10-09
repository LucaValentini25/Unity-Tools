#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Editor
{
    public class ReplaceForPrefabWindow : EditorWindow
    {
        // Tool variables
        public GameObject prefab; // The prefab to be replaced
        public GameObject parentToInstantiateChilds; // The parent object to instantiate new objects under
        public Transform[] objects; // Array of objects to be replaced

        public bool pos = true, rotation = true, scale = true; // Options to copy position, rotation, and scale

        public NameType nameType; // Enum for naming mode
        public ObjectsHandle objectsHandle; // Enum for handling objects
        public string customName = ""; // Custom name for the objects

        [FormerlySerializedAs("CopyComponents")] public bool copyComponents; // Option to copy components

        // Variables to handle foldouts
        private bool _showSettings; // Controls visibility of the settings foldout
        private bool _showTransformOptions; // Controls visibility of the transform options foldout
        private bool _showNameOptions; // Controls visibility of the name options foldout
        private bool _showComponentOptions; // Controls visibility of the component options foldout
        private bool _showObjOptions; // Controls visibility of the object handling options foldout

        public enum NameType
        {
            FromPrefab, // Name objects based on the prefab name
            FromObject, // Name objects based on the original object name
            Custom // Use a custom name for objects
        }

        public enum ObjectsHandle
        {
            Nothing, // Do nothing to objects
            Disable, // Deactivate objects
            Destroy // Destroy objects
        }
    
        // Scroll variables
        private Vector2 _scrollPosition; // Scroll position for the scroll view
        
        // Method to show the window
        [MenuItem("Tools/Replace For Prefab")]
        public static void ShowWindow()
        {
            GetWindow<ReplaceForPrefabWindow>("Replace For Prefab");
        }

        private void OnGUI()
        {
            // Start ScrollView
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

            // Field to select the prefab
            EditorGUILayout.LabelField("Prefab To Replace", EditorStyles.boldLabel);
            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
            if(!prefab)EditorGUILayout.HelpBox("Please Select a Prefab", MessageType.Error);
            EditorGUILayout.Space();

            // Field to select the parent object where new objects will be instantiated
            EditorGUILayout.LabelField("Parent of new objects", EditorStyles.boldLabel);
            parentToInstantiateChilds = (GameObject)EditorGUILayout.ObjectField("Parent to Instantiate", parentToInstantiateChilds, typeof(GameObject), true);
            if(!parentToInstantiateChilds)EditorGUILayout.HelpBox("If Parent is not selected Prefabs will instantiate on Hierarchy bottom", MessageType.Warning);
            EditorGUILayout.Space();
        
            // Field to select objects to replace
            EditorGUILayout.LabelField("Objects to Replace", EditorStyles.boldLabel);
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty objectsProperty = serializedObject.FindProperty("objects");
            EditorGUILayout.PropertyField(objectsProperty, true);
            EditorGUILayout.HelpBox("Objects Childs will not be copied", MessageType.Info);
            EditorGUILayout.Space();

            // Settings foldout
            _showSettings = EditorGUILayout.Foldout(_showSettings, "Settings", true, EditorStyles.foldoutHeader);
            if (_showSettings)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15); 
                EditorGUILayout.BeginVertical();

                EditorGUILayout.Space();
                // Transform options foldout
                #region TransformOptions
            
                _showTransformOptions = EditorGUILayout.Foldout(_showTransformOptions, "Copy From Transform", true, EditorStyles.foldoutHeader);
                if (_showTransformOptions)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15); // Indent options
                    pos = EditorGUILayout.ToggleLeft("Position", pos, GUILayout.Width(80));
                    rotation = EditorGUILayout.ToggleLeft("Rotation", rotation, GUILayout.Width(80));
                    scale = EditorGUILayout.ToggleLeft("Scale", scale, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                }

                #endregion

                EditorGUILayout.Space();
                // Name options foldout
                #region NameOptions

                _showNameOptions = EditorGUILayout.Foldout(_showNameOptions, "Name", true, EditorStyles.foldoutHeader);
                if (_showNameOptions)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15); // Indent options
                    EditorGUILayout.BeginVertical();

                    // NameType selector
                    nameType = (NameType)EditorGUILayout.EnumPopup("Naming Mode", nameType);

                    // Behavior based on selected name type
                    switch (nameType)
                    {
                        case NameType.FromPrefab:
                            if (prefab)
                            {
                                EditorGUILayout.LabelField("Name", prefab.name, EditorStyles.textField);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("Name", "Prefab not selected", EditorStyles.textField);
                            }
                            break;

                        case NameType.FromObject:
                            // No additional display
                            break;

                        case NameType.Custom:
                            customName = EditorGUILayout.TextField("Custom Name", customName);
                            break;
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                #endregion

                EditorGUILayout.Space();
                // Component options foldout
                #region ComponentOptions

                _showComponentOptions = EditorGUILayout.Foldout(_showComponentOptions, "ComponentsHandle", true, EditorStyles.foldoutHeader);
                if (_showComponentOptions)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15); // Indent options
                    copyComponents = EditorGUILayout.ToggleLeft("Copy Object Components", copyComponents);
                    EditorGUILayout.EndHorizontal();
                }

                #endregion

                EditorGUILayout.Space();
                // Object handling options foldout
                #region ObjectHandleOptions

                _showObjOptions = EditorGUILayout.Foldout(_showObjOptions, "Object Handle", true, EditorStyles.foldoutHeader);
                if (_showObjOptions)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15); 
                    EditorGUILayout.BeginVertical();

                    // ObjectsHandle selector
                    objectsHandle = (ObjectsHandle)EditorGUILayout.EnumPopup("Object Handle Mode", objectsHandle);

                    // Warning message based on selected option
                    switch (objectsHandle)
                    {
                        case ObjectsHandle.Nothing:
                            EditorGUILayout.HelpBox("Objects will be left unchanged.", MessageType.Info);
                            break;

                        case ObjectsHandle.Disable:
                            EditorGUILayout.HelpBox("Objects will be deactivated.", MessageType.Info);
                            break;

                        case ObjectsHandle.Destroy:
                            EditorGUILayout.HelpBox("Objects will be destroyed. This action cannot be Undo!", MessageType.Warning);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                #endregion
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
        
            // Button to perform the replacement
            if (GUILayout.Button("Replace"))
            {
                Replace();
            }

            serializedObject.ApplyModifiedProperties();
            
            // End ScrollView
            EditorGUILayout.EndScrollView();
        }

        // Method to replace objects with prefab
        private void Replace()
        {
            if (!prefab) return; // Exit if no prefab is selected
            string objName;
            foreach (var item in objects)
            {
                if (item)
                {
                    // Instantiate the prefab
                    GameObject obj = PrefabUtility.InstantiatePrefab(prefab, parentToInstantiateChilds ? parentToInstantiateChilds.transform : null) as GameObject;
                    if (!obj) continue;
                    
                    // Apply transform properties
                    #region Transform

                    if (pos && rotation) obj.transform.SetPositionAndRotation(item.position, item.rotation);
                    else
                    {
                        if (pos) obj.transform.position = item.position;
                        if (rotation) obj.transform.rotation = item.rotation;
                    }
                    if (scale) obj.transform.localScale = item.localScale;

                    #endregion
                
                    // Apply the name based on selected mode
                    #region Name

                    switch (nameType)
                    {
                        case NameType.FromPrefab:
                            objName = obj.name;
                            break;
                        case NameType.FromObject:
                            objName = item.name;
                            break;
                        case NameType.Custom:
                            objName = customName;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    obj.name = objName;

                    #endregion

                    // Copy components if enabled
                    #region Components

                    if (copyComponents)
                    {
                        CopyAllComponents(item.gameObject, obj);
                    }

                    #endregion
                
                    // Handle objects based on selected option
                    #region ObjHandle

                    switch (objectsHandle)
                    {
                        case ObjectsHandle.Nothing:
                            item.gameObject.SetActive(item.gameObject.activeSelf);
                            break;
                        case ObjectsHandle.Disable:
                            item.gameObject.SetActive(false);
                            break;
                        case ObjectsHandle.Destroy:
                            DestroyImmediate(item.gameObject);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    #endregion
                }
            }

            // Clear the array if objects are destroyed
            if (objectsHandle.Equals(ObjectsHandle.Destroy)) objects = new Transform[0];
        }

        // Method to copy all components from source to destination
        private void CopyAllComponents(GameObject source, GameObject destination)
        {
            Component[] components = source.GetComponents<Component>();
            foreach (Component component in components)
            {
                // Skip Transform, MeshFilter, and MeshRenderer components
                if (component is Transform or MeshFilter or MeshRenderer) continue;

                Type type = component.GetType();
                Component copy = destination.AddComponent(type);

                // Copy serialized values
                #region SerializationCopy

                SerializedObject serializedSource = new SerializedObject(component);
                SerializedObject serializedCopy = new SerializedObject(copy);

                SerializedProperty prop = serializedSource.GetIterator();
                while (prop.NextVisible(true))
                {
                    SerializedProperty copyProp = serializedCopy.FindProperty(prop.propertyPath);
                    if (copyProp != null)
                    {
                        // Copy property values based on type
                        switch (copyProp.propertyType)
                        {
                            case SerializedPropertyType.Integer:
                                copyProp.intValue = prop.intValue;
                                break;
                            case SerializedPropertyType.Float:
                                copyProp.floatValue = prop.floatValue;
                                break;
                            case SerializedPropertyType.String:
                                copyProp.stringValue = prop.stringValue;
                                break;
                            case SerializedPropertyType.Boolean:
                                copyProp.boolValue = prop.boolValue;
                                break;
                            case SerializedPropertyType.ObjectReference:
                                copyProp.objectReferenceValue = prop.objectReferenceValue;
                                break;
                            case SerializedPropertyType.Enum:
                                copyProp.enumValueIndex = prop.enumValueIndex;
                                break;
                            case SerializedPropertyType.Vector2:
                                copyProp.vector2Value = prop.vector2Value;
                                break;
                            case SerializedPropertyType.Vector3:
                                copyProp.vector3Value = prop.vector3Value;
                                break;
                            case SerializedPropertyType.Color:
                                copyProp.colorValue = prop.colorValue;
                                break;
                            case SerializedPropertyType.Bounds:
                                copyProp.boundsValue = prop.boundsValue;
                                break;
                            case SerializedPropertyType.Rect:
                                copyProp.rectValue = prop.rectValue;
                                break;
                            case SerializedPropertyType.ArraySize:
                                copyProp.intValue = prop.intValue;
                                break;
                            case SerializedPropertyType.Character:
                                copyProp.intValue = prop.intValue;
                                break;
                            case SerializedPropertyType.AnimationCurve:
                                copyProp.animationCurveValue = prop.animationCurveValue;
                                break;
                            case SerializedPropertyType.BoundsInt:
                                copyProp.boundsIntValue = prop.boundsIntValue;
                                break;
                            case SerializedPropertyType.RectInt:
                                copyProp.rectIntValue = prop.rectIntValue;
                                break;
                            case SerializedPropertyType.Quaternion:
                                copyProp.quaternionValue = prop.quaternionValue;
                                break;
                            case SerializedPropertyType.LayerMask:
                                copyProp.intValue = prop.intValue; // LayerMask is an int
                                break;
                            default:
                                Debug.LogWarning($"Unhandled SerializedPropertyType: {copyProp.propertyType}");
                                break;
                        }
                    }
                }
        
                serializedCopy.ApplyModifiedProperties();

                #endregion
            }
        }
    }
}
#endif
