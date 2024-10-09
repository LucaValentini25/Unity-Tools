using UnityEngine;
using UnityEditor;

public class RenameItems : EditorWindow
{
    private string baseName = "Item"; // Valor predeterminado
    private bool useCustomBaseName = false; // Bool para decidir si el baseName es custom o no
    private bool enumerate = false;
    private EnumerationFormat format = EnumerationFormat.Item1;
    public GameObject[] objectsToRename; // Hacemos que sea público para que aparezca en el Editor
    private string previewName = ""; // Para mostrar el formato según el enum seleccionado

    // Enum para definir los formatos de enumeración
    public enum EnumerationFormat
    {
        Item1,            // Item 1
        Item_1,           // Item_1
        ItemParentheses1  // Item (1)
    }

    [MenuItem("Tools/Rename Items")]
    public static void ShowWindow()
    {
        GetWindow<RenameItems>("Rename Items");
    }

    private void OnGUI()
    {
        // Mostrar el array de objetos para que el usuario pueda asignarlos manualmente
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty objectsProp = serializedObject.FindProperty("objectsToRename");

        // Desplegable para seleccionar los objetos
        EditorGUILayout.PropertyField(objectsProp, new GUIContent("Objects to Rename"), true);
        serializedObject.ApplyModifiedProperties();

        // Bool para decidir si el Base Name es custom o no, se coloca primero
        useCustomBaseName = EditorGUILayout.Toggle("Use Custom Base Name", useCustomBaseName);

        // Actualizar baseName solo si no es custom y hay un primer objeto
        if (objectsToRename != null && objectsToRename.Length > 0 && 
            !string.IsNullOrEmpty(objectsToRename[0]?.name) && !useCustomBaseName)
        {
            baseName = objectsToRename[0].name;
        }

        // Mostrar el Base Name como readonly si no es custom
        EditorGUI.BeginDisabledGroup(!useCustomBaseName); // Deshabilita el campo si no es custom
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        EditorGUI.EndDisabledGroup();

        // Checkbox para enumerar los objetos
        enumerate = EditorGUILayout.Toggle("Enumerate", enumerate);

        // Si se quiere enumerar, mostrar las opciones de formato
        if (enumerate)
        {
            format = (EnumerationFormat)EditorGUILayout.EnumPopup("Format", format);

        // Actualizar el preview del nombre basado en el formato seleccionado
        UpdatePreviewName();

        // Mostrar el campo readonly con el nombre previsualizado
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Preview", previewName);
        EditorGUI.EndDisabledGroup();
        }

        // Botón para ejecutar el renombrado
        if (GUILayout.Button("Rename Objects"))
        {
            RenameSelectedObjects();
        }
    }

    // Método para actualizar la vista previa del nombre basado en el formato seleccionado
    private void UpdatePreviewName()
    {
        previewName = baseName; // Inicia con el baseName

        if (enumerate)
        {
            // Simular el nombre del primer objeto enumerado (1)
            switch (format)
            {
                case EnumerationFormat.Item1:
                    previewName += " 1";
                    break;
                case EnumerationFormat.Item_1:
                    previewName += "_1";
                    break;
                case EnumerationFormat.ItemParentheses1:
                    previewName += " (1)";
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
            if (objectsToRename[i] != null) // Asegurarse de que el objeto no sea null
            {
                string newName = baseName;

                if (enumerate)
                {
                    switch (format)
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
}
