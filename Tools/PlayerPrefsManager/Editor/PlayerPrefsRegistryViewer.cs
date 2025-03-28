#if UNITY_EDITOR_WIN

using UnityEngine;
using Microsoft.Win32;
using System.Collections.Generic;

namespace com.ClasterTools.PlayerPrefsManager.Editor
{
    public static class PlayerPrefsRegistryViewer
    {
        public static List<PlayerPrefEntry> FetchPrefs()
        {
            List<PlayerPrefEntry> prefs = new();

            string company = Application.companyName;
            string product = Application.productName;
            string registryPath = $"Software\\Unity\\UnityEditor\\{company}\\{product}";

            RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath);
            if (key == null)
            {
                Debug.LogWarning($"⚠ No PlayerPrefs found at HKEY_CURRENT_USER\\{registryPath}");
                return prefs;
            }

            foreach (var valueName in key.GetValueNames())
            {
                string cleanKey = valueName.Split(new[] { "_h" }, System.StringSplitOptions.None)[0];
                object val = key.GetValue(valueName);
                RegistryValueKind kind = key.GetValueKind(valueName);

                string valueText;
                PlayerPrefType type;

                try
                {
                    switch (kind)
                    {
                        case RegistryValueKind.String:
                            valueText = val.ToString();
                            type = PlayerPrefType.String;
                            break;

                        case RegistryValueKind.DWord:
                            if (val is long longVal)
                            {
                                double floatVal = System.BitConverter.Int64BitsToDouble(longVal);
                                valueText = floatVal.ToString("0.######");
                                type = PlayerPrefType.Float;
                            }
                            else if (val is int rawInt)
                            {
                                float floatVal = System.BitConverter.ToSingle(System.BitConverter.GetBytes(rawInt), 0);
                                bool looksLikeFloat = floatVal is > 0.00001f or < -0.00001f;

                                if (looksLikeFloat && Mathf.Abs(rawInt - floatVal) > 0.01f)
                                {
                                    valueText = floatVal.ToString("0.######");
                                    type = PlayerPrefType.Float;
                                }
                                else
                                {
                                    valueText = rawInt.ToString();
                                    type = PlayerPrefType.Int;
                                }
                            }
                            else
                            {
                                valueText = "0";
                                type = PlayerPrefType.Int;
                            }

                            break;

                        case RegistryValueKind.QWord:
                            if (val is long longValue)
                            {
                                double floatVal = System.BitConverter.Int64BitsToDouble(longValue);
                                valueText = floatVal.ToString("0.######");
                                type = PlayerPrefType.Float;
                            }
                            else if (val is byte[] bytes && bytes.Length == 8)
                            {
                                long reconstructed = System.BitConverter.ToInt64(bytes, 0);
                                double floatVal = System.BitConverter.Int64BitsToDouble(reconstructed);
                                valueText = floatVal.ToString("0.######");
                                type = PlayerPrefType.Float;
                            }
                            else
                            {
                                valueText = "0";
                                type = PlayerPrefType.Float;
                            }

                            break;

                        case RegistryValueKind.Binary:
                            byte[] byteVal = val as byte[];
                            try
                            {
                                string decoded = System.Text.Encoding.UTF8.GetString(byteVal).TrimEnd('\0');
                                valueText = decoded;
                                type = PlayerPrefType.String;
                            }
                            catch
                            {
                                valueText = System.BitConverter.ToString(byteVal);
                                type = PlayerPrefType.String;
                            }

                            break;

                        default:
                            valueText = val?.ToString() ?? "";
                            type = PlayerPrefType.String;
                            break;
                    }

                    prefs.Add(new PlayerPrefEntry
                    {
                        key = cleanKey,
                        value = valueText,
                        type = type
                    });
                }
                catch
                {
                    Debug.LogWarning($"Failed to read PlayerPref key: {cleanKey}");
                }
            }

            key.Close();
            return prefs;
        }
    }
}
#endif
