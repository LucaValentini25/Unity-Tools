using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using static UnityEngine.Application;


public static class ToolExport
{
    private static string destinationPath;
    private static string[] CopyFolderNames = new string[3] { "Assets", "ProjectSettings", "Packages" };

    [MenuItem("Tools/Export/Export Project Files")]
    public static void ExportProjectFiles()
    {

       do
        {

            destinationPath = EditorUtility.SaveFolderPanel("Guardar archivos del proyecto", "C:", "");
            if (string.IsNullOrEmpty(destinationPath))
            {
                Debug.Log("No se ha seleccionado ninguna ubicaci�n de guardado.");
                break;
            }
        } while (!IsPathValid(destinationPath));
        
        string copyPath = CreateCopy(destinationPath);
        CompressDirectory(copyPath,destinationPath);
        Directory.Delete(copyPath, true );
    }
    private static bool IsPathValid(string destinationPath)
    {
        
        string projectPath = Directory.GetParent(dataPath).FullName;

        // Normalize paths to ensure correct comparison
        string normalizedDestinationPath = Path.GetFullPath(destinationPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string normalizedProjectPath = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (normalizedDestinationPath.StartsWith(normalizedProjectPath, StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Ubicación inválida", "El destino seleccionado está dentro de la carpeta del proyecto de Unity. Por favor, elige otra ubicación.", "OK");
            return false;
        }
        else
        {
            projectPath = Directory.GetParent(projectPath).FullName;
             normalizedDestinationPath = Path.GetFullPath(destinationPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
             normalizedProjectPath = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
             if (normalizedDestinationPath.StartsWith(normalizedProjectPath, StringComparison.OrdinalIgnoreCase))
             {
                 EditorUtility.DisplayDialog("Ubicación inválida", "El destino seleccionado está dentro de la carpeta del proyecto de Unity. Por favor, elige otra ubicación.", "OK");
				return false;
             }
        }
        return true;
    }
    private static string CreateCopy(string _destinationPath)
    {
        string savePath = Path.Combine(destinationPath + "/"+productName);
        Directory.CreateDirectory(savePath);
        string proyectPath = Directory.GetParent(dataPath).FullName;

        try
        {
            foreach (var directoryName in CopyFolderNames)
            {
            CopyDirectory(directoryName,proyectPath,savePath);
                
            }
            // CopyDirectory("Assets",proyectPath,savePath);
            // CopyDirectory("ProjectSettings", proyectPath, savePath);
            // CopyDirectory("Packages", proyectPath, savePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al copiar los archivos: " + ex.Message);
            return null;
        }
        AssetDatabase.Refresh();
        Debug.Log("Los archivos del proyecto han sido exportados exitosamente.");
        return savePath;
    }

    private static void CopyDirectory(string directoryName, string proyectPath, string savePath)
    {
        string sourceFolder = Path.Combine(proyectPath, directoryName);
        string destinationFolder = Path.Combine(savePath, directoryName);

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        // Copy all files
        string[] files = Directory.GetFiles(sourceFolder);
        foreach (string filePath in files)
        {
            try
            {
                using (FileStream sourceStream = File.OpenRead(filePath))
                {
                    string destinationFilePath = Path.Combine(destinationFolder, Path.GetFileName(filePath));

                    using (FileStream destinationStream = File.Create(destinationFilePath))
                    {
                        sourceStream.CopyTo(destinationStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error al copiar el archivo: " + Path.GetFileName(filePath) + ", " + ex.Message);
            }
        }

        // Copy all subdirectories
        string[] subDirectories = Directory.GetDirectories(sourceFolder);
        foreach (string subDirectory in subDirectories)
        {
            string subDirectoryName = Path.GetFileName(subDirectory);
            CopyDirectory(Path.Combine(directoryName, subDirectoryName), proyectPath, savePath);
        }
    }
    private static void CompressDirectory(string _savePath, string _destinationPath)
    {
        string zipFileName = Path.Combine(_destinationPath, "Export.zip");
        if (File.Exists(zipFileName))
        {
            File.Delete(zipFileName);
        }

        try
        {
            ZipFile.CreateFromDirectory(_savePath, zipFileName,System.IO.Compression.CompressionLevel.Optimal, true);
            Debug.Log("Carpeta comprimida exitosamente en: " + zipFileName);
        }
        catch (IOException ex)
        {
            Debug.LogError("Error al comprimir la carpeta: " + _savePath + ", " + ex.Message);
        }
    }
}
