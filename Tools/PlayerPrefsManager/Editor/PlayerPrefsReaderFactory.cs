using UnityEngine;
using System.Collections.Generic;

namespace com.ClasterTools.PlayerPrefsManager.Editor
{
    public static class PlayerPrefsReaderFactory
    {
        public static IPlayerPrefsReader Create()
        {
#if UNITY_EDITOR_WIN
            return new WindowsPlayerPrefsReader();
#elif UNITY_EDITOR_OSX
            return new MacOSPlayerPrefsReader();
#elif UNITY_EDITOR_LINUX
            return new LinuxPlayerPrefsReader();
#else
            Debug.LogWarning("PlayerPrefsManager: Unsupported platform. Falling back to empty reader.");
            return new NullPlayerPrefsReader();
#endif
        }
    }

    public class NullPlayerPrefsReader : IPlayerPrefsReader
    {
        public List<PlayerPrefEntry> FetchPrefs() => new();
    }
}
