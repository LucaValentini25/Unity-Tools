#if UNITY_EDITOR_LINUX

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace com.ClasterTools.PlayerPrefsManager.Editor
{
    public class LinuxPlayerPrefsReader : IPlayerPrefsReader
    {
        public List<PlayerPrefEntry> FetchPrefs()
        {
            var prefs = new List<PlayerPrefEntry>();

            string prefsPath = GetPrefsPath();
            if (string.IsNullOrEmpty(prefsPath) || !File.Exists(prefsPath))
            {
                Debug.LogWarning($"No PlayerPrefs file found at: {prefsPath}");
                return prefs;
            }

            ParsePrefsFile(prefsPath, prefs);
            return prefs;
        }

        private string GetPrefsPath()
        {
            string configDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrEmpty(configDir))
                configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config");

            return Path.Combine(configDir, "unity3d", Application.companyName, Application.productName, "prefs");
        }

        private void ParsePrefsFile(string path, List<PlayerPrefEntry> prefs)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(path);

                var prefNodes = doc.SelectNodes("//pref");
                if (prefNodes == null) return;

                foreach (XmlNode node in prefNodes)
                {
                    var entry = ParsePrefNode(node);
                    if (entry != null) prefs.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse Linux PlayerPrefs: {ex.Message}");
            }
        }

        private PlayerPrefEntry ParsePrefNode(XmlNode node)
        {
            string key = node.Attributes?["name"]?.Value;
            if (string.IsNullOrEmpty(key)) return null;

            string typeStr = node.Attributes?["type"]?.Value ?? "string";
            string value = node.InnerText;

            PlayerPrefType type = typeStr switch
            {
                "int" => PlayerPrefType.Int,
                "float" => PlayerPrefType.Float,
                _ => PlayerPrefType.String
            };

            return new PlayerPrefEntry { key = key, value = value, type = type };
        }
    }
}

#endif
