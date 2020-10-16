using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ganss.XSS;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TwitchBot.Service.Models;
using TwitchBot.Service.Pages;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Services
{
    public class TwitchBot
    {
        private readonly TwitchConfig _config;
        private readonly TwitchClient _client;
        private readonly HubConnection _hubConnection;
        private readonly ILogger<TwitchBot> _logger;
        private readonly TwitchAPI _twitchApiClient;
        private readonly HtmlSanitizer _sanitizer;
        private readonly IMapper _mapper;
        private readonly MemoryCache _cache;

        public TwitchBot(
            IOptions<TwitchConfig> config, 
            ILogger<TwitchBot> logger, 
            HubConnection hubConnection, 
            HtmlSanitizer sanitizer,
            TwitchMemoryCache twitchMemoryCache,
            IMapper mapper)
        {
            _config = config.Value ?? throw new MissingConfigException();
            _hubConnection = hubConnection;
            _hubConnection.StartAsync();
            _logger = logger;
            _sanitizer = sanitizer;
            _mapper = mapper;
            _cache = twitchMemoryCache.Cache;

            var credentials = new ConnectionCredentials(
                _config.Chat.BotName,
                _config.Chat.PaswordGeneratorToken);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            var apiSettings = new ApiSettings()
            {
                ClientId = _config.Auth.ClientId,
                Secret = _config.Auth.ClientSecret,
            };

            _twitchApiClient = new TwitchAPI(settings: apiSettings);

            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, _config.Chat.Channel);
            
            _client.OnConnected += ClientOnConnected;
            _client.OnJoinedChannel += ClientOnJoinedChannel;
            _client.OnChatCommandReceived += ClientOnChatCommandReceived;
            _client.OnMessageReceived += ClientOnMessageReceived;

            _client.Connect();
        }

        private async void ClientOnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (
                (e.ChatMessage.Message.Trim().StartsWith("!")) ||
                (_config.IgnoredUsers.Any(iu => iu.Equals(e.ChatMessage.Username, StringComparison.InvariantCultureIgnoreCase)))
               ) return;

            var isTeamMember = _config.TeamMembers.Any(tm =>
                    tm.Id.Equals(e.ChatMessage.UserId, StringComparison.InvariantCultureIgnoreCase));

            if (isTeamMember && !_cache.TryGetValue($"TeamShoutout:{e.ChatMessage.UserId}", out string cacheEntry))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                var channel = await _twitchApiClient.V5.Channels.GetChannelByIDAsync(e.ChatMessage.UserId);
                var message = @$"{_config.TeamName} team member detected! 
    YEET!, @{e.ChatMessage.Username}! 
    Check out their channel here: https://twitch.tv/{e.ChatMessage.Username} 
    | They were last seen streaming {channel.Status} in {channel.Game}";
                _client.SendMessage(_config.Chat.Channel, message);
                _cache.Set($"TeamShoutout:{e.ChatMessage.UserId}", cacheEntry, cacheEntryOptions);
            }
            var sanitizedHtml = _sanitizer.Sanitize(e.ChatMessage.Message);
            var emotes = _mapper.Map<IEnumerable<EmoteDto>>(e.ChatMessage.EmoteSet.Emotes);

            var userTypes = UserTypes.None;
            if (e.ChatMessage.IsBroadcaster) userTypes |= UserTypes.Broadcaster;
            if (e.ChatMessage.IsModerator) userTypes |= UserTypes.Moderator;
            if (e.ChatMessage.IsSubscriber) userTypes |= UserTypes.Subscriber;
            if (e.ChatMessage.IsVip) userTypes |= UserTypes.Vip;
            if (_config.TeamMembers.Any(tm =>
                tm.Id.Equals(e.ChatMessage.UserId, StringComparison.InvariantCultureIgnoreCase)))
                userTypes |= UserTypes.TeamMember;

            var data = new ChatMessageData
            {
                MessageId = e.ChatMessage.Id,
                UserId = e.ChatMessage.UserId,
                UserName = e.ChatMessage.Username,
                DisplayName = e.ChatMessage.DisplayName,
                Message = sanitizedHtml,
                TeamName = _config.TeamName,
                TeamShoutoutEnabled = _config.TeamShoutoutEnabled,
                EmoteDetails = emotes.ToArray(),
                UserTypes = userTypes
            };

            await _hubConnection.InvokeAsync("SendChatMessage", data);
        }

        private void OnFollow(object sender, OnFollowArgs e)
        {
            _logger.LogInformation($"{JsonConvert.SerializeObject(e, Formatting.Indented)}");
        }

        private async void ClientOnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            _logger.LogInformation($"Receieved Chat Command: {JsonConvert.SerializeObject(e.Command, Formatting.Indented)}");

            switch (e.Command.CommandText)
            {
                case "drop":
                    await _hubConnection.InvokeAsync("SendMessage", e.Command.ChatMessage.DisplayName, "Make it rain!!!");
                    break;
                case "yeet":
                    var param = e.Command.ArgumentsAsList.Any() ? e.Command.ArgumentsAsString : "yourself";
                    _client.SendMessage(_config.Chat.Channel, $"You yeeted {param} into tomorrow!");
                    break;
                default:
                    if (_config.Chat.RespondToUnknownCommand)
                        _client.SendMessage(_config.Chat.Channel, $"y33t you! {e.Command.CommandText} obvs doesn't exist! n00b!");
                    break;
            }
        }

        private void ClientOnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            _logger.LogInformation($"Connected to Channel: {e.Channel}");
            _client.SendMessage(e.Channel, "Hello peeps! Yeet all the thingz!");
        }

        private void ClientOnConnected(object sender, OnConnectedArgs e)
        {
            _logger.LogInformation($"Client Connected... {e.BotUsername}");
        }
    }
}
