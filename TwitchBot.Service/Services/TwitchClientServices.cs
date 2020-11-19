using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Models;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TwitchClientServices> _logger;
        public TwitchClient Client { get; private set; }
        private readonly TwitchConfig _config;

        public TwitchClientServices(IOptions<TwitchConfig> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = loggerFactory.CreateLogger<TwitchClientServices>();
            _config = settings.Value!;
        }

        public void Init()
        {
            var credentials = new ConnectionCredentials(
                _config.Chat.BotName,
                _config.Chat.PasswordGeneratorToken);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                ReconnectionPolicy = new ReconnectionPolicy(1000)
            };

            var customClient = new WebSocketClient(clientOptions);
            Client = new TwitchClient(customClient);
            Client.Initialize(credentials, _config.Chat.Channel);

            Client.OnConnected += ClientOnConnected;
            Client.OnDisconnected += ClientOnDisconnected;
            Client.OnJoinedChannel += ClientOnJoinedChannel;
        }

        public void Connect()
        {
            Client.Connect();
        }

        // UPDATE Redemption Status
        // https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions?broadcaster_id=190920462&id=36e78953-b405-40a5-b995-13ea19d0dce6&reward_id=a35bc0d0-8409-4d6b-8c98-0f2c174f3296
        public async Task UpdateChannelRedemptionStatus(string id, string rewardId, RedemptionStatus status)
        {
            const string broadcasterId = "190920462";
            var url = $"/helix/channel_points/custom_rewards/redemptions?broadcaster_id={broadcasterId}&id={id}&reward_id={rewardId}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.Auth.AccessToken);
            request.Headers.Add("client-id", _config.Auth.ClientId);
            request.Content = JsonContent.Create(new
            {
                status = status.ToString()
            });
            var httpClient = _httpClientFactory.CreateClient("TwitchClientServices");
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private void ClientOnConnected(object sender, OnConnectedArgs e)
        {
            _logger.LogInformation($"Client Connected... {e.BotUsername}");
        }

        private void ClientOnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            _logger.LogInformation($"Connected to Channel: {e.Channel}");
        }

        private void ClientOnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            _logger.LogInformation($"Connected disconnected. reconnecting...");
            if (!Client.IsConnected) Client.Connect();
        }

    }

    public enum RedemptionStatus
    {
        CANCELED, 
        FULFILLED,
        UNFULFILLED
    }
}