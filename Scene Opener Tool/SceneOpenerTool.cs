#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneOpenerToolInitializer
{
    public const string TargetSceneKey = "SceneOpenerTool_TargetScene";
    public const string AutoOpenSceneKey = "SceneOpenerTool_AutoOpenScene";

    static SceneOpenerToolInitializer()
    {
        EditorApplication.projectChanged += OnProjectChanged;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        OnProjectChanged();  // Ejecutar inmediatamente al cargar el proyecto
    }

    private static void OnProjectChanged()
    {
        CheckAndOpenScene();
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        CheckAndOpenScene();
    }

    private static void CheckAndOpenScene()
    {
        if (!EditorPrefs.GetBool(AutoOpenSceneKey, true))
            return;

        string scenePath = EditorPrefs.GetString(TargetSceneKey, "");
        if (string.IsNullOrEmpty(scenePath))
            return;

        bool isTargetSceneOpen = false;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath)
            {
                isTargetSceneOpen = true;
                break;
            }
        }

        if (!isTargetSceneOpen)
        {
            OpenTargetScene(scenePath);
        }
    }

    private static void OpenTargetScene(string scenePath)
    {
        if (!string.IsNullOrEmpty(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }
        else
        {
            Debug.LogWarning("Scene not found!");
        }
    }
}

public class SceneOpenerTool : EditorWindow
{
    private SceneAsset targetScene;
    private bool autoOpenScene = true;
    [MenuItem("Tools/Scene Opener Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneOpenerTool>("Scene Opener Tool");
        window.minSize = new Vector2(300, 150); // Establece un tamaño mínimo por defecto
        window.position = new Rect(window.position.x, window.position.y, window.minSize.x, window.minSize.y);
    }

    private void OnEnable()
    {
        // Cargar configuraciones guardadas
        string scenePath = EditorPrefs.GetString(SceneOpenerToolInitializer.TargetSceneKey, "");
        if (!string.IsNullOrEmpty(scenePath))
        {
            targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }

        autoOpenScene = EditorPrefs.GetBool(SceneOpenerToolInitializer.AutoOpenSceneKey, true);
    }

    private void OnDisable()
    {
        // Guardar configuraciones
        if (targetScene != null)
        {
            EditorPrefs.SetString(SceneOpenerToolInitializer.TargetSceneKey, AssetDatabase.GetAssetPath(targetScene));
        }
        EditorPrefs.SetBool(SceneOpenerToolInitializer.AutoOpenSceneKey, autoOpenScene);
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Opener Settings", EditorStyles.boldLabel);

        // Campo para seleccionar la escena desde el editor
        targetScene = (SceneAsset)EditorGUILayout.ObjectField("Target Scene", targetScene, typeof(SceneAsset), false);
        GUILayout.Space(10);

        // Botón para activar/desactivar la apertura automática
        if (autoOpenScene)
        {
            if (GUILayout.Button("Disable Automatic Open Scene"))
            {
                autoOpenScene = false;
            }
        }
        else
        {
            if (GUILayout.Button("Enable Automatic Open Scene"))
            {
                autoOpenScene = true;
            }
        }
        
        GUILayout.Space(10);

        // Mostrar la ruta de la escena seleccionada en un HelpBox
        if (targetScene != null)
        {
            EditorGUILayout.HelpBox($"Target Scene Path: {AssetDatabase.GetAssetPath(targetScene)}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No scene selected.", MessageType.Warning);
        }

    }

    private void OpenTargetScene()
    {
        if (targetScene == null) return;

        string scenePath = AssetDatabase.GetAssetPath(targetScene);
        if (!string.IsNullOrEmpty(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }
        else
        {
            Debug.LogWarning("Scene not found!");
        }
    }
}

#endif