using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Trashy.Twitch
{
    public class ChatService : MonoBehaviour
    {
        private ItemSpawner _itemSpawner;
        private TwitchClient _client;
        private bool _userDisconnected;
        private TwitchToken _token;

        public bool IsConnected { get; private set; }

        public async Task Connect()
        {
            _userDisconnected = false;
            _token = await TwitchAuth.Validate();
            if (_token != null)
            {
                Setup();
                _client.Connect();
            }
        }

        public void Disconnect()
        {
            _userDisconnected = true;
            if (_client != null)
                _client.Disconnect();
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
            if (_client != null)
            {
                _client.OnConnected -= OnConnected;
                _client.OnDisconnected -= OnDisconnected;
                _client.OnReconnected -= OnReconnected;
                _client.OnError -= OnError;
                _client.OnMessageReceived -= OnMessageReceived;
                _client.OnNewSubscriber -= OnNewSubscriber;
                _client.OnReSubscriber -= OnReSubscriber;
                _client.OnGiftedSubscription -= OnGiftedSubscription;
                _client.Disconnect();
            }

            _client = new TwitchClient();
            _client.OnConnected += OnConnected;
            _client.OnDisconnected += OnDisconnected;
            _client.OnReconnected += OnReconnected;
            _client.OnError += OnError;
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnNewSubscriber += OnNewSubscriber;
            _client.OnReSubscriber += OnReSubscriber;
            _client.OnGiftedSubscription += OnGiftedSubscription;
            // Community gifts trigger gifted sub for each gift so not using OnCommunitySubscription

            _client.Initialize(
                new ConnectionCredentials($"justinfan{Random.Range(1000, 900000)}", ""),
                _token.Login
            );
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            Log.Info<ChatService>("Connected");
            IsConnected = true;
        }

        private void OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            Log.Info<ChatService>("Disconnected");
            IsConnected = false;

            if (!_userDisconnected)
            {
                Log.Info<ChatService>("Trying to reconnect");
                Setup();
                _client.Connect();
            }
        }

        private void OnReconnected(object sender, OnReconnectedEventArgs e)
        {
            // Working around a reconnect bug in TwitchLib
            ((TwitchClient)sender).Disconnect();
        }

        private void OnError(object sender, OnErrorEventArgs e)
        {
            Log.Error<ChatService>(e.Exception.ToString());
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            foreach (var trigger in ConfigManager.Triggers.Where(x => x.Enabled && x.Type == TriggerType.Command))
            {
                if (!e.ChatMessage.Message.ToLower().StartsWith(trigger.CommandName.ToLower()))
                    continue;

                var allowTrigger = false;
                switch (trigger.CommandRestriction)
                {
                    case CommandRestriction.Everyone:
                        allowTrigger = true;
                        break;

                    case CommandRestriction.Subscriber:
                        allowTrigger = e.ChatMessage.IsSubscriber || e.ChatMessage.IsVip ||
                                       e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster;
                        break;

                    case CommandRestriction.Vip:
                        allowTrigger = e.ChatMessage.IsVip ||
                                       e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster;
                        break;

                    case CommandRestriction.Moderator:
                        allowTrigger = e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster;
                        break;
                }

                var isOnCooldown = DateTimeOffset.Now - trigger.CommandLastTrigger <
                                   TimeSpan.FromSeconds(trigger.CommandCooldown);

                // Broadcaster bypasses the cooldown
                if (allowTrigger && (!isOnCooldown || e.ChatMessage.IsBroadcaster))
                {
                    trigger.CommandLastTrigger = DateTimeOffset.Now;
                    ThreadingHelper.Instance.StartSyncInvoke(() => _itemSpawner.SpawnTrash(trigger));
                }
            }

            if (e.ChatMessage.Bits > 0)
            {
                foreach (var trigger in ConfigManager.Triggers.Where(x => x.Enabled && x.Type == TriggerType.Bits))
                {
                    if (e.ChatMessage.Bits >= trigger.MinAmount)
                    {
                        ThreadingHelper.Instance.StartSyncInvoke(() =>
                            _itemSpawner.SpawnTrash(trigger)
                        );
                    }
                }
            }
        }

        private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            foreach (var trigger in ConfigManager.Triggers.Where(x => x.Enabled && x.Type == TriggerType.Sub))
            {
                ThreadingHelper.Instance.StartSyncInvoke(() =>
                    _itemSpawner.SpawnTrash(trigger)
                );
            }
        }

        private void OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            foreach (var trigger in ConfigManager.Triggers.Where(x => x.Enabled && x.Type == TriggerType.Sub))
            {
                ThreadingHelper.Instance.StartSyncInvoke(() =>
                    _itemSpawner.SpawnTrash(trigger)
                );
            }
        }

        private void OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            foreach (var trigger in ConfigManager.Triggers.Where(x => x.Enabled && x.Type == TriggerType.GiftSub))
            {
                ThreadingHelper.Instance.StartSyncInvoke(() =>
                    _itemSpawner.SpawnTrash(trigger)
                );
            }
        }
    }
}
