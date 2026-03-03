using System.Collections.Generic;

namespace com.ClasterTools.PlayerPrefsManager.Editor
{
    public interface IPlayerPrefsReader
    {
        List<PlayerPrefEntry> FetchPrefs();
    }
}
