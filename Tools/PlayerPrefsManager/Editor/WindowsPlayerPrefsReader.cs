#if UNITY_EDITOR_WIN

using UnityEngine;
using Microsoft.Win32;
using System.Collections.Generic;

namespace com.ClasterTools.PlayerPrefsManager.Editor
{
    public class WindowsPlayerPrefsReader : IPlayerPrefsReader
    {
        public List<PlayerPrefEntry> FetchPrefs()
        {
            var prefs = new List<PlayerPrefEntry>();

            string company = Application.companyName;
            string product = Application.productName;
            string registryPath = $"Software\\Unity\\UnityEditor\\{company}\\{product}";

            RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath);
            if (key == null)
            {
                Debug.LogWarning($"No PlayerPrefs found at HKEY_CURRENT_USER\\{registryPath}");
                return prefs;
            }

            foreach (var valueName in key.GetValueNames())
            {
                string cleanKey = valueName.Split(new[] { "_h" }, System.StringSplitOptions.None)[0];
                object val = key.GetValue(valueName);
                RegistryValueKind kind = key.GetValueKind(valueName);

                try
                {
                    var entry = ParseRegistryValue(cleanKey, val, kind);
                    if (entry != null) prefs.Add(entry);
                }
                catch
                {
                    Debug.LogWarning($"Failed to read PlayerPref key: {cleanKey}");
                }
            }

            key.Close();
            return prefs;
        }

        private PlayerPrefEntry ParseRegistryValue(string cleanKey, object val, RegistryValueKind kind)
        {
            string valueText;
            PlayerPrefType type;

            switch (kind)
            {
                case RegistryValueKind.String:
                    valueText = val.ToString();
                    type = PlayerPrefType.String;
                    break;

                case RegistryValueKind.DWord:
                    (valueText, type) = ParseDWord(val);
                    break;

                case RegistryValueKind.QWord:
                    (valueText, type) = ParseQWord(val);
                    break;

                case RegistryValueKind.Binary:
                    (valueText, type) = ParseBinary(val);
                    break;

                default:
                    valueText = val?.ToString() ?? "";
                    type = PlayerPrefType.String;
                    break;
            }

            return new PlayerPrefEntry { key = cleanKey, value = valueText, type = type };
        }

        private (string, PlayerPrefType) ParseDWord(object val)
        {
            if (val is long longVal)
            {
                double floatVal = System.BitConverter.Int64BitsToDouble(longVal);
                return (floatVal.ToString("0.######"), PlayerPrefType.Float);
            }

            if (val is int rawInt)
            {
                float floatVal = System.BitConverter.ToSingle(System.BitConverter.GetBytes(rawInt), 0);
                bool looksLikeFloat = floatVal is > 0.00001f or < -0.00001f;

                if (looksLikeFloat && Mathf.Abs(rawInt - floatVal) > 0.01f)
                    return (floatVal.ToString("0.######"), PlayerPrefType.Float);

                return (rawInt.ToString(), PlayerPrefType.Int);
            }

            return ("0", PlayerPrefType.Int);
        }

        private (string, PlayerPrefType) ParseQWord(object val)
        {
            if (val is long longValue)
            {
                double floatVal = System.BitConverter.Int64BitsToDouble(longValue);
                return (floatVal.ToString("0.######"), PlayerPrefType.Float);
            }

            if (val is byte[] bytes && bytes.Length == 8)
            {
                long reconstructed = System.BitConverter.ToInt64(bytes, 0);
                double floatVal = System.BitConverter.Int64BitsToDouble(reconstructed);
                return (floatVal.ToString("0.######"), PlayerPrefType.Float);
            }

            return ("0", PlayerPrefType.Float);
        }

        private (string, PlayerPrefType) ParseBinary(object val)
        {
            if (val is byte[] byteVal)
            {
                try
                {
                    string decoded = System.Text.Encoding.UTF8.GetString(byteVal).TrimEnd('\0');
                    return (decoded, PlayerPrefType.String);
                }
                catch
                {
                    return (System.BitConverter.ToString(byteVal), PlayerPrefType.String);
                }
            }

            return ("", PlayerPrefType.String);
        }
    }
}

#endif
