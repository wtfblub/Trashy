using BepInEx.Configuration;

namespace Trashy
{
    public static class ConfigManager
    {
        public static ConfigEntry<int> ObjectCount;
        public static ConfigEntry<int> SpriteSize;
        public static ConfigEntry<string> RewardName;
        public static ConfigEntry<string> TwitchToken;
    }
}
