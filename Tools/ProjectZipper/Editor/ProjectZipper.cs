using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace com.ClasterTools.ProjectZipper.Editor
{
    public static class ProjectZipper
    {
        private static readonly string[] RequiredFolders = { "Assets", "ProjectSettings", "Packages" };

        [MenuItem("Tools/Export to Zip")]
        public static void ExportProject()
        {
            var zipFilePath = "";
            do
            {
                
                var defaultZipName = Application.productName + ".zip";
                zipFilePath = EditorUtility.SaveFilePanel(
                    "Export Project Backup",
                    "",
                    defaultZipName,
                    "zip");

            if (string.IsNullOrEmpty(zipFilePath)) return;

            } while (!IsPathValid(zipFilePath));

            var tempPath = Path.Combine(Path.GetTempPath(), "UnityProjectBackupTemp");

            try
            {
                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);

                var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                var totalFolders = RequiredFolders.Length;

                for (var i = 0; i < totalFolders; i++)
                {
                    var folderName = RequiredFolders[i];
                    if (projectRoot == null) continue;
                    var sourcePath = Path.Combine(projectRoot, folderName);
                    var targetPath = Path.Combine(tempPath, folderName);

                    EditorUtility.DisplayProgressBar("Copying Folders", folderName, (float)i / totalFolders);
                    CopyDirectory(sourcePath, targetPath);
                }

                EditorUtility.DisplayProgressBar("Creating Zip", Path.GetFileName(zipFilePath), 1f);
                if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
                ZipFile.CreateFromDirectory(tempPath, zipFilePath, CompressionLevel.Optimal, true);

                Debug.Log("Project backup created at: " + zipFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("ProjectZipper Error: " + ex.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var targetFilePath = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var targetSubDir = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, targetSubDir);
            }
        }

        private static bool IsPathValid(string path)
        {
            var fullName = Directory.GetParent(Application.dataPath)?.FullName;
            if (fullName == null) return true;
            var projectRoot = Path.GetFullPath(fullName)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToLowerInvariant();

            var targetDir = Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToLowerInvariant();

            // Solo invalidar si el destino está DENTRO del proyecto
            if (!targetDir.StartsWith(projectRoot + Path.DirectorySeparatorChar)) return true;
            EditorUtility.DisplayDialog("Invalid Path",
                "Cannot export inside the Unity project folder. Please choose another location.",
                "OK");
            return false;

        }
    }
}
