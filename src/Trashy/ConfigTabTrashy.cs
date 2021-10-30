using Trashy.Twitch;
using UnityEngine;

namespace Trashy
{
    public class ConfigTabTrashy : ConfigWindowTab
    {
        private const int MaxObjects = 100;
        private readonly SpriteManager _spriteManager;
        private readonly CanvasGroup _hotkeysCanvasGroup;
        private readonly TwitchRedeems _twitchRedeems;
        private TwitchToken _twitchToken;

        public ConfigTabTrashy()
        {
            WindowID = 4;
            _spriteManager = GetComponent<SpriteManager>();
            _hotkeysCanvasGroup = FindObjectOfType<ConfigTab_Hotkeys>().GetComponent<CanvasGroup>();
            _twitchRedeems = GetComponent<TwitchRedeems>();
        }

        private new void Awake()
        {
            // Overwrite ConfigWindowTab.Awake since our gameobject does not have the Components this is looking for
        }

        public override async void Initialize(int selectedTab)
        {
            setIsSelected(selectedTab);
            CanvasGroup.alpha = isSelected ? 1 : 0;
            _twitchToken = await TwitchAuth.Validate();
        }

        public override void SetPlatformLayout(RuntimePlatform platform)
        {
        }

        public override void ClosedConfig()
        {
        }

        private void OnGUI()
        {
            if (!isSelected)
                return;

            if (UIManager.Skin != null)
                GUI.skin = UIManager.Skin;

            GUI.Window(
                0,
                new Rect(Screen.width - 520, 100, 430, 400),
                DrawConfigWindow,
                $"Trashy Configuration - {TrashyPlugin.Version}"
            );
        }

        private void Update()
        {
            if (!isSelected)
                return;

            if (_hotkeysCanvasGroup.alpha > 0)
                _hotkeysCanvasGroup.alpha = 0;
        }

        private void OnDestroy()
        {
            TwitchAuth.CancelLogin();
        }

        private void DrawConfigWindow(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(
                    "Object Count:",
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 10, 0) },
                    GUILayout.Width(150)
                );
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(
                        ConfigManager.ObjectCount.Value.ToString(),
                        new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                    );
                    ConfigManager.ObjectCount.Value =
                        Mathf.RoundToInt(
                            GUILayout.HorizontalSlider(ConfigManager.ObjectCount.Value, 1, MaxObjects)
                        );
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(
                    "Sprite Size:",
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 10, 0) },
                    GUILayout.Width(150)
                );
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(
                        ConfigManager.SpriteSize.Value.ToString(),
                        new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                    );
                    ConfigManager.SpriteSize.Value =
                        32 * (Mathf.RoundToInt(
                            GUILayout.HorizontalSlider(ConfigManager.SpriteSize.Value, 512, 6400)
                        ) / 32);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(
                    "Sticky Chance:",
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 10, 0) },
                    GUILayout.Width(150)
                );
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(
                        $"{ConfigManager.StickyChance.Value}%",
                        new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                    );
                    ConfigManager.StickyChance.Value =
                        Mathf.RoundToInt(
                            GUILayout.HorizontalSlider(ConfigManager.StickyChance.Value, 0, 100)
                        );
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(
                    "Sticky Duration:",
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 10, 0) },
                    GUILayout.Width(150)
                );
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(
                        $"{ConfigManager.StickyDuration.Value} seconds",
                        new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                    );
                    ConfigManager.StickyDuration.Value =
                        (float)System.Math.Round(
                            GUILayout.HorizontalSlider(ConfigManager.StickyDuration.Value, 0.3f, 2f),
                            1
                        );
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("Reload Items"))
                _spriteManager.Load();

            GUILayout.Space(20);
            if (TwitchAuth.IsValidating)
            {
                GUILayout.Label("Validating Twitch Token...");
            }
            else
            {
                if (TwitchAuth.IsWaitingForLogin)
                {
                    DrawWaitingForLogin();
                }
                else
                {
                    if (_twitchToken == null)
                        DrawLoggedOut();
                    else
                        DrawLoggedIn();
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawWaitingForLogin()
        {
            GUILayout.Label("Waiting for login...");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Open Login Page"))
                    Application.OpenURL(TwitchAuth.GetLoginUrl());

                if (GUILayout.Button("Cancel"))
                    TwitchAuth.CancelLogin();
            }
            GUILayout.EndHorizontal();
        }

        private async void DrawLoggedOut()
        {
            if (GUILayout.Button("Connect with Twitch"))
            {
                if (await TwitchAuth.Login())
                {
                    _twitchToken = await TwitchAuth.Validate();
                    if (_twitchToken != null)
                        await _twitchRedeems.Connect();
                }
            }
        }

        private void DrawLoggedIn()
        {
            GUILayout.Label(
                $"Twitch Channel: {_twitchToken.Login}",
                new GUIStyle(GUI.skin.label) { margin = new RectOffset(0, 0, 6, 0) }
            );

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(
                    "Twitch Reward Name:",
                    new GUIStyle(GUI.skin.label) { margin = new RectOffset(0, 0, 6, 0) },
                    GUILayout.Width(150)
                );

                ConfigManager.RewardName.Value = GUILayout.TextField(ConfigManager.RewardName.Value);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("Logout"))
            {
                _ = TwitchAuth.Revoke();
                _twitchRedeems.Disconnect();
                _twitchToken = null;
            }

            GUILayout.Space(20);
            DrawPubSubStatus();
        }

        private async void DrawPubSubStatus()
        {
            if (!_twitchRedeems.IsConnected)
            {
                GUILayout.Label("Disconnected from Twitch");
                if (GUILayout.Button("Connect"))
                    await _twitchRedeems.Connect();
            }
        }
    }
}
