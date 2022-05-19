using Trashy.Twitch;
using UnityEngine;

namespace Trashy.UI
{
    public class GeneralConfigWindow : Window
    {
        private readonly SpriteManager _spriteManager;
        private readonly PubSubService _pubSubService;
        private readonly ChatService _chatService;
        private TwitchToken _twitchToken;
        private Vector2 _scrollPosition;

        public GeneralConfigWindow(SpriteManager spriteManager, PubSubService pubSubService, ChatService chatService)
        {
            _spriteManager = spriteManager;
            _pubSubService = pubSubService;
            _chatService = chatService;
        }

        protected override async void OnIsOpenChanged(bool isOpen)
        {
            if (isOpen)
            {
                _twitchToken = await TwitchAuth.Validate();
            }
            else
            {
                UIManager.GetWindow<TriggersWindow>().IsOpen = false;
                TwitchAuth.CancelLogin();
            }
        }

        public override void OnDraw()
        {
            GUI.Window(
                Id,
                new Rect(Screen.width - 520, 100, 430, 400),
                DrawWindow,
                $"Trashy Configuration - {TrashyPlugin.Version}"
            );
        }

        private async void DrawWindow(int windowId)
        {
            GUILayout.Space(30);
            using (GUILayoutEx.ScrollView(ref _scrollPosition))
            using (GUILayoutEx.VerticalScope())
            {
                GUILayoutEx.ConfigIntSlider(
                    "Item size:",
                    ConfigManager.SpriteSize,
                    512,
                    6400,
                    32
                );

                GUILayoutEx.NewConfigEntry("Model reaction:", () =>
                {
                    if (GUILayout.Button(ConfigManager.ManipulateModel.Value ? "on" : "off", GUILayout.Width(35)))
                        ConfigManager.ManipulateModel.Value = !ConfigManager.ManipulateModel.Value;
                });

                if (ConfigManager.ManipulateModel.Value)
                {
                    GUILayoutEx.ConfigIntSlider(
                        "      Reaction power:",
                        ConfigManager.ManipulateModelPower,
                        10,
                        100,
                        steps: 5
                    );
                }

                GUILayoutEx.NewConfigEntry("Play hit sound:", () =>
                {
                    if (GUILayout.Button(ConfigManager.PlayHitSound.Value ? "on" : "off", GUILayout.Width(35)))
                        ConfigManager.PlayHitSound.Value = !ConfigManager.PlayHitSound.Value;
                });

                if (ConfigManager.PlayHitSound.Value)
                {
                    GUILayoutEx.ConfigIntSlider(
                        "      Volume:",
                        ConfigManager.HitSoundVolume,
                        1,
                        100,
                        steps: 1
                    );
                }

                GUILayout.Space(5);

                if (GUILayout.Button("Edit Triggers"))
                {
                    var triggersWindow = UIManager.GetWindow<TriggersWindow>();
                    triggersWindow.IsOpen = !triggersWindow.IsOpen;
                }

                if (GUILayout.Button("Reload Items"))
                    _spriteManager.Load();

                if (GUILayout.Button("Reload hit sounds"))
                    await SoundManager.LoadAudioClips();

                if (TwitchAuth.IsValidating)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("Validating Twitch Token...");
                }
                else
                {
                    if (TwitchAuth.IsWaitingForLogin)
                    {
                        GUILayout.Space(10);
                        TwitchWaitingForLogin();
                    }
                    else
                    {
                        if (_twitchToken == null)
                        {
                            TwitchLoggedOut();
                        }
                        else
                        {
                            GUILayout.Space(10);
                            TwitchLoggedIn();
                        }
                    }
                }
            }
        }

        private void TwitchWaitingForLogin()
        {
            GUILayout.Label("Waiting for login...");
            GUILayout.Space(5);

            using (GUILayoutEx.HorizontalScope())
            {
                if (GUILayout.Button("Open Login Page"))
                    Application.OpenURL(TwitchAuth.GetLoginUrl());

                if (GUILayout.Button("Cancel"))
                    TwitchAuth.CancelLogin();
            }
        }

        private async void TwitchLoggedOut()
        {
            if (GUILayout.Button("Connect with Twitch"))
            {
                if (await TwitchAuth.Login())
                {
                    _twitchToken = await TwitchAuth.Validate();
                    if (_twitchToken != null)
                    {
                        await _pubSubService.Connect();
                        await _chatService.Connect();
                    }
                }
            }
        }

        private void TwitchLoggedIn()
        {
            GUILayout.Label(
                $"Twitch Channel: {_twitchToken.Login}",
                new GUIStyle(GUI.skin.label) { margin = new RectOffset(0, 0, 6, 0) }
            );

            GUILayout.Space(5);
            if (GUILayout.Button("Logout"))
            {
                _ = TwitchAuth.Revoke();
                _pubSubService.Disconnect();
                _chatService.Disconnect();
                _twitchToken = null;
            }

            GUILayout.Space(20);
            TwitchStatus();
        }

        private async void TwitchStatus()
        {
            if (!_pubSubService.IsConnected)
            {
                GUILayout.Label("Disconnected from Twitch PubSub");
                if (GUILayout.Button("Connect"))
                    await _pubSubService.Connect();
            }

            if (!_chatService.IsConnected)
            {
                GUILayout.Label("Disconnected from Twitch Chat");
                if (GUILayout.Button("Connect"))
                    await _chatService.Connect();
            }
        }
    }
}
