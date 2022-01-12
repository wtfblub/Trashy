using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;

namespace Trashy
{
    public enum TriggerType
    {
        Redeem,
        Bits,
        Sub,
        GiftSub
    }

    public class TriggerConfig
    {
        public bool Enabled;
        public TriggerType Type;
        public string RedeemName = "";
        public int MinAmount = 10;

        public int ItemCount = 10;
        public int StickyChance = 25;
        public float StickyDuration = 0.5f;

        public string ItemGroup = "";
    }

    public static class ConfigManager
    {
        public static ConfigEntry<int> SpriteSize;
        public static ConfigEntry<string> TwitchToken;
        public static ConfigEntry<bool> ManipulateModel;
        public static ConfigEntry<int> ManipulateModelPower;
        public static ConfigEntry<bool> PlayHitSound;
        public static ConfigEntry<int> HitSoundVolume;

        public static List<TriggerConfig> Triggers;

        public static void Initialize(ConfigFile config)
        {
            LoadTriggers();
            config.SaveOnConfigSet = true;

            Migrate1(config);

            SpriteSize = config.Bind(
                "General",
                "SpriteSize",
                1280,
                "The size of sprites"
            );
            TwitchToken = config.Bind(
                "General",
                "TwitchToken",
                "",
                "The Twitch Api Token"
            );
            ManipulateModel = config.Bind(
                "General",
                "ManipulateModel",
                true,
                "Whether the model should react to items or not"
            );
            ManipulateModelPower = config.Bind(
                "General",
                "ManipulateModelPower",
                40,
                "How strong the model reacts to items"
            );
            PlayHitSound = config.Bind(
                "General",
                "PlayHitSound",
                true,
                "If a hit sound should be played when items are hitting the model"
            );
            HitSoundVolume = config.Bind(
                "General",
                "HitSoundVolume",
                100,
                "Volume for the hit sound"
            );
        }

        public static void SaveTriggers()
        {
            var fileName = Path.Combine(Paths.ConfigPath, "TrashyPlugin_triggers.json");
            try
            {
                var json = JsonConvert.SerializeObject(Triggers, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to save triggers: {ex}");
            }
        }

        private static void LoadTriggers()
        {
            var fileName = Path.Combine(Paths.ConfigPath, "TrashyPlugin_triggers.json");
            if (!File.Exists(fileName))
            {
                Triggers = new List<TriggerConfig>();
                return;
            }

            try
            {
                Triggers = JsonConvert.DeserializeObject<List<TriggerConfig>>(File.ReadAllText(fileName)) ??
                           new List<TriggerConfig>();
            }
            catch (Exception ex)
            {
                Triggers = new List<TriggerConfig>();
                Log.Error($"Unable to load triggers: {ex}");
            }
        }

        private static void Migrate1(ConfigFile config)
        {
            // 0.2.1 -> 0.3.0
            var migrated1 = config.Bind(
                "Migration",
                "Migrated1",
                false
            );

            if (migrated1.Value)
                return;

            var objectCount = config.Bind(
                "General",
                "ObjectCount",
                10,
                "Unused since 0.3.0"
            );
            var rewardName = config.Bind(
                "General",
                "RewardName",
                "",
                "Unused since 0.3.0"
            );
            var stickyChance = config.Bind(
                "General",
                "StickyChance",
                25,
                "Unused since 0.3.0"
            );
            var stickyDuration = config.Bind(
                "General",
                "StickyDuration",
                0.5f,
                "Unused since 0.3.0"
            );

            Triggers.Add(new TriggerConfig
            {
                Enabled = true,
                Type = TriggerType.Redeem,
                RedeemName = rewardName.Value,
                ItemCount = objectCount.Value,
                StickyChance = stickyChance.Value,
                StickyDuration = stickyDuration.Value
            });
            SaveTriggers();
            migrated1.Value = true;
        }
    }
}
