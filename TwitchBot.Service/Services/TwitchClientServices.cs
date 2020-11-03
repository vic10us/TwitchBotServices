using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace TwitchBot.Service.Services
{
    public class TwitchClientServices
    {
        private readonly ILogger<TwitchClientServices> _logger;
        public TwitchClient Client { get; }

        public TwitchClientServices(IOptions<TwitchConfig> config, ILogger<TwitchClientServices> logger)
        {
            var config1 = config.Value;
            _logger = logger;

            var credentials = new ConnectionCredentials(
                config1.Chat.BotName,
                config1.Chat.PasswordGeneratorToken);
            
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            var customClient = new WebSocketClient(clientOptions);
            Client = new TwitchClient(customClient);
            Client.Initialize(credentials, config1.Chat.Channel);

            Client.OnConnected += ClientOnConnected;
            Client.OnDisconnected += ClientOnDisconnected;
            Client.OnJoinedChannel += ClientOnJoinedChannel;
            Client.Connect();
        }
        private void ClientOnConnected(object sender, OnConnectedArgs e)
        {
            _logger.LogInformation($"Client Connected... {e.BotUsername}");
        }

        private void ClientOnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            _logger.LogInformation($"Connected to Channel: {e.Channel}");
            Client.SendMessage(e.Channel, "Hello peeps! Yeet all the thingz!");
        }

        private void ClientOnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            _logger.LogInformation($"Connected disconnected. reconnecting...");
            if (!Client.IsConnected) Client.Connect();
        }
    }
}