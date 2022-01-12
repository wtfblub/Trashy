using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Trashy.UI;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using UnityEngine;

namespace Trashy.Twitch
{
    public class PubSubService : MonoBehaviour
    {
        private ItemSpawner _itemSpawner;
        private TwitchPubSub _pubSub;
        private bool _userDisconnected;
        private TwitchToken _token;

        public bool IsConnected { get; private set; }

        public PubSubService()
        {
            Setup();
        }

        public async Task Connect()
        {
            _userDisconnected = false;
            _token = await TwitchAuth.Validate();
            if (_token == null)
            {
                UIManager.GetWindow<MessageWindow>().Show(
                    "Trashy - Twitch not connected",
                    "Trashy is not connected with your Twitch Account anymore!\n" +
                    "You can connect your Twitch Account in the Settings."
                );
            }
            else
            {
                _pubSub.Connect();
            }
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

        private void Setup()
        {
            if (_pubSub != null)
            {
                _pubSub.OnPubSubServiceConnected -= OnPubSubServiceConnected;
                _pubSub.OnPubSubServiceClosed -= OnPubSubServiceClosed;
                _pubSub.OnPubSubServiceError -= OnPubSubServiceError;
                _pubSub.OnRewardRedeemed -= OnRewardRedeemed;
                _pubSub.Disconnect();
            }

            _pubSub = new TwitchPubSub();
            _pubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
            _pubSub.OnPubSubServiceClosed += OnPubSubServiceClosed;
            _pubSub.OnPubSubServiceError += OnPubSubServiceError;
            _pubSub.OnRewardRedeemed += OnRewardRedeemed;
        }

        private void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            Log.Info<PubSubService>("Connected");
            IsConnected = true;
            _pubSub.ListenToRewards(_token.UserId.ToString());
            _pubSub.SendTopics();
        }

        private void OnPubSubServiceClosed(object sender, EventArgs e)
        {
            Log.Info<PubSubService>("Disconnected");
            IsConnected = false;

            if (!_userDisconnected)
            {
                Log.Info<PubSubService>("Trying to reconnect");
                Setup();
                _pubSub.Connect();
            }
        }

        private void OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
        {
            Log.Error<PubSubService>(e.Exception.ToString());
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            foreach (var trigger in ConfigManager.Triggers.Where(x => x.Enabled && x.Type == TriggerType.Redeem))
            {
                if (e.RewardTitle.Equals(trigger.RedeemName, StringComparison.OrdinalIgnoreCase))
                {
                    ThreadingHelper.Instance.StartSyncInvoke(() =>
                        _itemSpawner.SpawnTrash(trigger)
                    );
                }
            }
        }
    }
}
