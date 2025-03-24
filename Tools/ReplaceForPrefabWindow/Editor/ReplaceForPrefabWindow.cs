#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.ClasterTools.ReplaceForPrefab.Editor
{
    public class ReplaceForPrefabWindow : EditorWindow
    {
        public GameObject prefab;
        public GameObject parentToInstantiateChilds;
        public Transform[] objects;

        public bool pos = true, rotation = true, scale = true;

        public NameType nameType;
        public ObjectsHandle objectsHandle;
        public string customName = "";

        [FormerlySerializedAs("CopyComponents")] public bool copyComponents;

        private bool _showSettings;
        private bool _showTransformOptions;
        private bool _showNameOptions;
        private bool _showComponentOptions;
        private bool _showObjOptions;

        private Vector2 _scrollPosition;

        public enum NameType
        {
            FromPrefab,
            FromObject,
            Custom
        }

        public enum ObjectsHandle
        {
            Nothing,
            Disable,
            Destroy
        }

        [MenuItem("Tools/Replace For Prefab")]
        public static void ShowWindow()
        {
            GetWindow<ReplaceForPrefabWindow>("Replace For Prefab");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.HelpBox(
                "This tool replaces selected objects in the scene with instances of a prefab, copying transform data and optionally name and components.",
                MessageType.Info);

            EditorGUILayout.LabelField("Prefab To Replace", EditorStyles.boldLabel);
            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
            if (!prefab) EditorGUILayout.HelpBox("Please Select a Prefab", MessageType.Error);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Parent of new objects", EditorStyles.boldLabel);
            parentToInstantiateChilds = (GameObject)EditorGUILayout.ObjectField("Parent to Instantiate", parentToInstantiateChilds, typeof(GameObject), true);
            if (!parentToInstantiateChilds) EditorGUILayout.HelpBox("If Parent is not selected, Prefabs will instantiate at root", MessageType.Warning);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Objects to Replace", EditorStyles.boldLabel);
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty objectsProperty = serializedObject.FindProperty("objects");
            EditorGUILayout.PropertyField(objectsProperty, true);
            if (GUILayout.Button("Add Selected Objects"))
            {
                objects = Selection.transforms;
            }
            EditorGUILayout.HelpBox("Children of selected objects will not be copied.", MessageType.Info);

            EditorGUILayout.Space();
            _showSettings = EditorGUILayout.Foldout(_showSettings, "Settings", true);
            if (_showSettings)
            {
                EditorGUILayout.BeginVertical("box");

                _showTransformOptions = EditorGUILayout.Foldout(_showTransformOptions, "Copy From Transform", true);
                if (_showTransformOptions)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    pos = EditorGUILayout.ToggleLeft("Position", pos, GUILayout.Width(80));
                    rotation = EditorGUILayout.ToggleLeft("Rotation", rotation, GUILayout.Width(80));
                    scale = EditorGUILayout.ToggleLeft("Scale", scale, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                }

                _showNameOptions = EditorGUILayout.Foldout(_showNameOptions, "Name", true);
                if (_showNameOptions)
                {
                    EditorGUILayout.BeginVertical();
                    nameType = (NameType)EditorGUILayout.EnumPopup("Naming Mode", nameType);

                    switch (nameType)
                    {
                        case NameType.FromPrefab:
                            EditorGUILayout.LabelField("Name", prefab ? prefab.name : "Prefab not selected");
                            break;
                        case NameType.Custom:
                            customName = EditorGUILayout.TextField("Custom Name", customName);
                            break;
                    }
                    EditorGUILayout.EndVertical();
                }

                _showComponentOptions = EditorGUILayout.Foldout(_showComponentOptions, "Components", true);
                if (_showComponentOptions)
                {
                    copyComponents = EditorGUILayout.ToggleLeft("Copy Object Components", copyComponents);
                }

                _showObjOptions = EditorGUILayout.Foldout(_showObjOptions, "Object Handle", true);
                if (_showObjOptions)
                {
                    objectsHandle = (ObjectsHandle)EditorGUILayout.EnumPopup("Object Handle Mode", objectsHandle);

                    switch (objectsHandle)
                    {
                        case ObjectsHandle.Nothing:
                            EditorGUILayout.HelpBox("Objects will be left unchanged.", MessageType.Info);
                            break;
                        case ObjectsHandle.Disable:
                            EditorGUILayout.HelpBox("Objects will be deactivated.", MessageType.Info);
                            break;
                        case ObjectsHandle.Destroy:
                            EditorGUILayout.HelpBox("Objects will be destroyed. This action cannot be undone!", MessageType.Warning);
                            break;
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            GUI.enabled = prefab != null && objects != null && objects.Length > 0;
            if (GUILayout.Button("Replace"))
            {
                Replace();
            }
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        private void Replace()
        {
            foreach (var item in objects)
            {
                if (!item) continue;

                GameObject obj = PrefabUtility.InstantiatePrefab(prefab, parentToInstantiateChilds ? parentToInstantiateChilds.transform : null) as GameObject;
                if (!obj) continue;

                if (pos && rotation) obj.transform.SetPositionAndRotation(item.position, item.rotation);
                else
                {
                    if (pos) obj.transform.position = item.position;
                    if (rotation) obj.transform.rotation = item.rotation;
                }
                if (scale) obj.transform.localScale = item.localScale;

                switch (nameType)
                {
                    case NameType.FromPrefab:
                        obj.name = prefab.name;
                        break;
                    case NameType.FromObject:
                        obj.name = item.name;
                        break;
                    case NameType.Custom:
                        obj.name = customName;
                        break;
                }

                if (copyComponents)
                {
                    CopyAllComponents(item.gameObject, obj);
                }

                switch (objectsHandle)
                {
                    case ObjectsHandle.Nothing:
                        break;
                    case ObjectsHandle.Disable:
                        item.gameObject.SetActive(false);
                        break;
                    case ObjectsHandle.Destroy:
                        DestroyImmediate(item.gameObject);
                        break;
                }
            }

            if (objectsHandle == ObjectsHandle.Destroy) objects = new Transform[0];
        }

        private void CopyAllComponents(GameObject source, GameObject destination)
        {
            Component[] components = source.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component is Transform or MeshFilter or MeshRenderer) continue;

                Type type = component.GetType();
                Component copy = destination.AddComponent(type);

                SerializedObject serializedSource = new SerializedObject(component);
                SerializedObject serializedCopy = new SerializedObject(copy);

                SerializedProperty prop = serializedSource.GetIterator();
                while (prop.NextVisible(true))
                {
                    SerializedProperty copyProp = serializedCopy.FindProperty(prop.propertyPath);
                    if (copyProp == null) continue;

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
                        case SerializedPropertyType.AnimationCurve:
                            copyProp.animationCurveValue = prop.animationCurveValue;
                            break;
                        case SerializedPropertyType.Quaternion:
                            copyProp.quaternionValue = prop.quaternionValue;
                            break;
                    }
                }

                serializedCopy.ApplyModifiedProperties();
            }
        }
    }
}
#endif