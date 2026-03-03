#if UNITY_EDITOR_OSX

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Debug = UnityEngine.Debug;

namespace com.ClasterTools.PlayerPrefsManager.Editor
{
    public class MacOSPlayerPrefsReader : IPlayerPrefsReader
    {
        public List<PlayerPrefEntry> FetchPrefs()
        {
            var prefs = new List<PlayerPrefEntry>();

            string plistPath = GetPlistPath();
            if (string.IsNullOrEmpty(plistPath) || !System.IO.File.Exists(plistPath))
            {
                Debug.LogWarning($"No PlayerPrefs plist found at: {plistPath}");
                return prefs;
            }

            string xml = ConvertPlistToXml(plistPath);
            if (string.IsNullOrEmpty(xml))
                return prefs;

            ParsePlistXml(xml, prefs);
            return prefs;
        }

        private string GetPlistPath()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string bundleId = $"unity.{Application.companyName}.{Application.productName}";
            return System.IO.Path.Combine(home, "Library", "Preferences", $"{bundleId}.plist");
        }

        private string ConvertPlistToXml(string plistPath)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "plutil",
                        Arguments = $"-convert xml1 -o - \"{plistPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode == 0 ? output : null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to convert plist: {ex.Message}");
                return null;
            }
        }

        private void ParsePlistXml(string xml, List<PlayerPrefEntry> prefs)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var dict = doc.SelectSingleNode("//dict");
            if (dict == null) return;

            var children = dict.ChildNodes;
            for (int i = 0; i < children.Count - 1; i++)
            {
                var keyNode = children[i];
                if (keyNode.Name != "key") continue;

                var valueNode = children[i + 1];
                string rawKey = keyNode.InnerText;
                string cleanKey = StripHash(rawKey);

                var entry = CreateEntry(cleanKey, valueNode);
                if (entry != null) prefs.Add(entry);
                i++;
            }
        }

        private PlayerPrefEntry CreateEntry(string key, XmlNode valueNode)
        {
            switch (valueNode.Name)
            {
                case "string":
                    return new PlayerPrefEntry { key = key, value = valueNode.InnerText, type = PlayerPrefType.String };

                case "integer":
                    return new PlayerPrefEntry { key = key, value = valueNode.InnerText, type = PlayerPrefType.Int };

                case "real":
                    return new PlayerPrefEntry { key = key, value = valueNode.InnerText, type = PlayerPrefType.Float };

                default:
                    return new PlayerPrefEntry { key = key, value = valueNode.InnerText, type = PlayerPrefType.String };
            }
        }

        private string StripHash(string rawKey)
        {
            int index = rawKey.LastIndexOf("_h", StringComparison.Ordinal);
            return index > 0 ? rawKey.Substring(0, index) : rawKey;
        }
    }
}

#endif
