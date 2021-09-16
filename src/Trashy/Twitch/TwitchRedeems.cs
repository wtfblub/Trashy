using System;
using System.Threading.Tasks;
using BepInEx;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using UnityEngine;

namespace Trashy.Twitch
{
    public class TwitchRedeems : MonoBehaviour
    {
        private ItemSpawner _itemSpawner;
        private TwitchPubSub _pubSub;
        private bool _userDisconnected;
        private TwitchToken _token;

        private bool _showDisconnectedPopup;

        public bool IsConnected { get; private set; }

        public TwitchRedeems()
        {
            Setup();
        }

        public async Task Connect()
        {
            _userDisconnected = false;
            _token = await TwitchAuth.Validate();
            if (_token == null)
                _showDisconnectedPopup = true;
            else
                _pubSub.Connect();
        }

        public void Disconnect()
        {
            _userDisconnected = true;
            _pubSub.Disconnect();
        }

        private async void Start()
        {
            _itemSpawner = GetComponent<ItemSpawner>();
            await Connect();
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnGUI()
        {
            if (!_showDisconnectedPopup)
                return;

            if (UIManager.Skin != null)
                GUI.skin = UIManager.Skin;

            const int windowWidth = 400;
            const int windowHeight = 165;
            GUI.Window(1,
                new Rect(Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight),
                DrawPopup,
                "Trashy - Twitch not connected"
            );
        }

        private void DrawPopup(int windowId)
        {
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            {
                GUILayout.Label(
                    "Trashy is not connected with your Twitch Account anymore!\n" +
                    "You can connect your Twitch Account in the Settings."
                );

                GUILayout.Space(20);
                if (GUILayout.Button("OK", GUILayout.Width(125)))
                    _showDisconnectedPopup = false;
            }
            GUILayout.EndVertical();
        }

        private void Setup()
        {
            if (_pubSub != null)
            {
                _pubSub.OnPubSubServiceConnected -= OnPubSubServiceConnected;
                _pubSub.OnPubSubServiceClosed -= OnPubSubServiceClosed;
                _pubSub.OnPubSubServiceError -= OnPubSubServiceError;
                _pubSub.OnRewardRedeemed -= OnRewardRedeemed;
            }

            _pubSub = new TwitchPubSub();
            _pubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
            _pubSub.OnPubSubServiceClosed += OnPubSubServiceClosed;
            _pubSub.OnPubSubServiceError += OnPubSubServiceError;
            _pubSub.OnRewardRedeemed += OnRewardRedeemed;
        }

        private void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            Log.Info<TwitchRedeems>("Connected");
            IsConnected = true;
            _pubSub.ListenToRewards(_token.UserId.ToString());
            _pubSub.SendTopics();
        }

        private void OnPubSubServiceClosed(object sender, EventArgs e)
        {
            Log.Info<TwitchRedeems>("Disconnected");
            IsConnected = false;

            if (!_userDisconnected)
            {
                Log.Info<TwitchRedeems>("Trying to reconnect");
                _pubSub.Disconnect();
                Setup();
                _pubSub.Connect();
            }
        }

        private void OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
        {
            Log.Error<TwitchRedeems>(e.Exception.ToString());
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            if (e.RewardTitle.Equals(ConfigManager.RewardName.Value, StringComparison.OrdinalIgnoreCase))
                ThreadingHelper.Instance.StartSyncInvoke(() => _itemSpawner.SpawnTrash());

            Log.Info<TwitchRedeems>(e.RewardTitle);
        }
    }
}
