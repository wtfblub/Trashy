using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Trashy.UI
{
    public class TriggersWindow : Window
    {
        private readonly ItemSpawner _itemSpawner;
        private readonly SpriteManager _spriteManager;
        private Vector2 _scrollPosition;
        private string[] _triggerTypeNames;

        public TriggersWindow(ItemSpawner itemSpawner, SpriteManager spriteManager)
        {
            _itemSpawner = itemSpawner;
            _spriteManager = spriteManager;
            _triggerTypeNames = Enum.GetNames(typeof(TriggerType));
        }

        public override void OnDraw()
        {
            GUI.Window(
                Id,
                new Rect(Screen.width - 955, 100, 430, 600),
                DrawWindow,
                "Trashy - Triggers"
            );
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.Space(20);
            using (GUILayoutEx.ScrollView(ref _scrollPosition))
            using (GUILayoutEx.VerticalScope())
            {
                using (GUILayoutEx.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    var selected = 0;
                    var clicked = GUILayoutEx.DropDownButton(
                        "Add trigger",
                        ref selected,
                        _triggerTypeNames,
                        GUILayout.ExpandWidth(false)
                    );
                    if (clicked)
                    {
                        ConfigManager.Triggers.Add(new TriggerConfig
                        {
                            Type = (TriggerType)Enum.Parse(typeof(TriggerType), _triggerTypeNames[selected])
                        });
                        ConfigManager.SaveTriggers();
                    }
                }

                GUILayoutEx.Separator();
                GUILayout.Space(5);

                IEnumerable<TriggerConfig> triggers = ConfigManager.Triggers;
                var saveTriggers = false;
                foreach (var config in triggers.Reverse())
                {
                    GUILayout.Space(20);
                    if (DrawTriggerConfig(config))
                        saveTriggers = true;

                    GUILayout.Space(5);
                    GUILayoutEx.Separator();
                }

                if (saveTriggers)
                    ConfigManager.SaveTriggers();
            }
        }

        private bool DrawTriggerConfig(TriggerConfig config)
        {
            var saveConfig = false;

            using (GUILayoutEx.HorizontalScope())
            {
                GUILayout.Label(config.Type.ToString(), new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    ConfigManager.Triggers.Remove(config);
                    return true;
                }
            }

            GUILayout.Space(10);
            GUILayoutEx.NewConfigEntry("Enabled:", () =>
            {
                if (GUILayout.Button(config.Enabled ? "on" : "off", GUILayout.Width(35)))
                {
                    config.Enabled = !config.Enabled;
                    saveConfig = true;
                }
            });

            GUILayout.Space(10);
            switch (config.Type)
            {
                case TriggerType.Redeem:
                    GUILayoutEx.NewConfigEntry("Redeem Name:", () =>
                    {
                        var newName = GUILayout.TextField(config.RedeemName);
                        if (!config.RedeemName.Equals(newName))
                        {
                            config.RedeemName = newName;
                            saveConfig = true;
                        }
                    });
                    break;

                case TriggerType.Bits:
                    GUILayoutEx.NewConfigEntry("Minimum Bits:", () =>
                    {
                        var newValueStr = GUILayout.TextField(config.MinAmount.ToString());
                        if (uint.TryParse(newValueStr, out var newValue))
                        {
                            if (config.MinAmount != newValue)
                            {
                                config.MinAmount = (int)newValue;
                                saveConfig = true;
                            }
                        }
                    });
                    break;
            }

            GUILayout.Space(10);
            var newItemCount = GUILayoutEx.ConfigIntSlider("Item Count:", config.ItemCount, 1, 100);
            if (config.ItemCount != newItemCount)
            {
                config.ItemCount = newItemCount;
                saveConfig = true;
            }

            GUILayout.Space(10);
            var newStickyChance = GUILayoutEx.ConfigIntSlider(
                "Sticky chance:",
                config.StickyChance,
                0,
                100,
                formatLabel: x => $"{x}%"
            );
            if (config.StickyChance != newStickyChance)
            {
                config.StickyChance = newStickyChance;
                saveConfig = true;
            }

            GUILayout.Space(10);
            var newStickyDuration = GUILayoutEx.ConfigFloatSlider(
                "Sticky duration:",
                config.StickyDuration,
                0.3f,
                2f,
                x => $"{x} seconds"
            );
            if (config.StickyDuration != newStickyDuration)
            {
                config.StickyDuration = newStickyDuration;
                saveConfig = true;
            }

            if (_spriteManager.Groups.Count > 0)
            {
                GUILayout.Space(10);
                GUILayoutEx.NewConfigEntry("Item Group:", () =>
                {
                    var preview = "<None>";
                    if (!string.IsNullOrWhiteSpace(config.ItemGroup))
                    {
                        // The group probably got deleted so update the config
                        if (!_spriteManager.Groups.ContainsKey(config.ItemGroup))
                        {
                            config.ItemGroup = "";
                            preview = "<None>";
                            saveConfig = true;
                        }
                        else
                        {
                            preview = config.ItemGroup;
                        }
                    }

                    var selectedIndex = 0;
                    var items = Enumerable.Repeat("<None>", 1).Concat(_spriteManager.Groups.Keys).ToArray();
                    if (GUILayoutEx.DropDownButton(preview, ref selectedIndex, items))
                    {
                        config.ItemGroup = selectedIndex == 0 ? "" : items[selectedIndex];
                        saveConfig = true;
                    }
                });
            }

            GUILayout.Space(10);
            using (GUILayoutEx.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Test"))
                {
                    IReadOnlyList<Sprite> sprites = _spriteManager.Items;
                    if (!string.IsNullOrWhiteSpace(config.ItemGroup))
                    {
                        if (_spriteManager.Groups.TryGetValue(config.ItemGroup, out var groupSprites))
                            sprites = groupSprites;
                    }

                    _itemSpawner.SpawnTrash(config, sprites);
                }
            }

            return saveConfig;
        }
    }
}
