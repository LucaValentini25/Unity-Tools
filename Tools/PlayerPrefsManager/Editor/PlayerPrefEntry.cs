namespace com.ClasterTools.PlayerPrefsManager.Editor
{
    public enum PlayerPrefType { Int, Float, String }

    public class PlayerPrefEntry
    {
        public string key = "";
        public string value = "";
        public PlayerPrefType type = PlayerPrefType.String;
    }
}
