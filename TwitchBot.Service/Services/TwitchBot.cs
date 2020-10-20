﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ganss.XSS;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OBS.WebSocket.Client;
using TwitchBot.Service.Models;
using TwitchLib.Api;
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
        private readonly OBSWebsocket _obs;
        private readonly OBSConfig _obsConfig;

        public TwitchBot(
            IOptions<TwitchConfig> config,
            IOptions<OBSConfig> obsConfig,
            ILogger<TwitchBot> logger, 
            HubConnection hubConnection, 
            HtmlSanitizer sanitizer,
            TwitchMemoryCache twitchMemoryCache,
            TwitchAPI twitchApi,
            IMapper mapper,
            OBSWebsocket obs)
        {
            _config = config.Value ?? throw new MissingConfigException();
            _hubConnection = hubConnection;
            _hubConnection.StartAsync();
            _logger = logger;
            _sanitizer = sanitizer;
            _mapper = mapper;
            _cache = twitchMemoryCache.Cache;
            _obs = obs;
            _obsConfig = obsConfig.Value;

            var credentials = new ConnectionCredentials(
                _config.Chat.BotName,
                _config.Chat.PasswordGeneratorToken);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            _twitchApiClient = twitchApi;

            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, _config.Chat.Channel);
            
            _client.OnConnected += ClientOnConnected;
            _client.OnJoinedChannel += ClientOnJoinedChannel;
            _client.OnChatCommandReceived += ClientOnChatCommandReceived;
            _client.OnMessageReceived += ClientOnMessageReceived;

            _client.Connect();

            SetupOBS();
        }

        private void SetupOBS()
        {
            _obs.Connected += OnObsConnected;
            _obs.Connect(_obsConfig.Connection.Url, _obsConfig.Connection.Password);
        }

        private void OnObsConnected(object sender, EventArgs e)
        {
            _logger.LogInformation("Connected to OBS");
            var sceneList = _obs.GetSceneList();

            var mainScene = sceneList.Scenes.FirstOrDefault(s => s.Name.Equals("Main", StringComparison.InvariantCultureIgnoreCase));
            if (mainScene != null)
            {
                var motionFilter = _obs.GetSourceFilterInfo(mainScene.Name, "Motion");

                if (motionFilter != null)
                {
                    var json = JsonConvert.SerializeObject(motionFilter.Settings, Formatting.Indented);
                    var x = JsonConvert.DeserializeObject<MotionFilter>(json);
                    _logger.LogInformation(json);
                }
            }

            foreach (var scene in sceneList.Scenes)
            {
                _logger.LogInformation($"Found Scene: {scene.Name}");
            }
        }

        private async void ClientOnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (
                (e.ChatMessage.Message.Trim().StartsWith("!")) ||
                (_config.IgnoredUsers.Any(iu => iu.Equals(e.ChatMessage.Username, StringComparison.InvariantCultureIgnoreCase)))
               ) return;

            var teamMember = _config.TeamMembers.FirstOrDefault(tm =>
                tm.Id.Equals(e.ChatMessage.UserId, StringComparison.InvariantCultureIgnoreCase));

            if (teamMember != null && !teamMember.IgnoreShoutOut && !_cache.TryGetValue($"TeamShoutout:{e.ChatMessage.UserId}", out string cacheEntry))
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
            var sanitizedHtml = System.Net.WebUtility.HtmlDecode(_sanitizer.Sanitize(e.ChatMessage.Message));
            // System.Net.WebUtility.HtmlDecode(sanitizedHtml);
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
            _logger.LogInformation($"Received Chat Command: {JsonConvert.SerializeObject(e.Command, Formatting.Indented)}");
            var commandUser = (await _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { $"{e.Command.ChatMessage.UserId}" })).Users.FirstOrDefault();

            switch (e.Command.CommandText.ToLowerInvariant())
            {
                case "drop":
                    await _hubConnection.InvokeAsync("SendMessage", e.Command.ChatMessage.DisplayName, "Make it rain!!!");
                    break;
                case "yeet":
                    var param = e.Command.ArgumentsAsList.Any() ? e.Command.ArgumentsAsString : "yourself";
                    _client.SendMessage(_config.Chat.Channel, $"You yeeted {param} into tomorrow!");
                    break;
                case "stats":
                    var user = (await _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> {"vic10usx"})).Users.FirstOrDefault();
                    var channel = await _twitchApiClient.V5.Channels.GetChannelByIDAsync(user.Id);
                    var streams = await _twitchApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> {user.Id});
                    var subs = await _twitchApiClient.Helix.Subscriptions.GetBroadcasterSubscriptions(user.Id);
                    _client.SendMessage(_config.Chat.Channel, $"Channel Views: {channel.Views}, Channel Followers: {channel.Followers}, Viewer Count: {streams.Streams.FirstOrDefault()?.ViewerCount ?? 0}, Subscriptions: {subs.Data.Count()}");
                    break;
                case "grow":
                    if (e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster)
                    {
                        TriggerHotkeyByName("Backward [ Motion ] ");
                    }
                    break;
                case "shrink":
                    if (e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster)
                    {
                        TriggerHotkeyByName("Forward [ Motion ] ");
                    }
                    break;
                default:
                    if (_config.Chat.RespondToUnknownCommand)
                        _client.SendMessage(
                            _config.Chat.Channel, 
                            $"y33t you! {e.Command.CommandText} obvs doesn't exist! n00b!"
                        );
                    break;
            }
        }

        /// <summary>
        /// Set the current scene to the specified one
        /// </summary>
        /// <param name="sceneName">The desired scene name</param>
        public void TriggerHotkeyByName(string hotKeyName)
        {
            var requestFields = new JObject { { "hotkeyName", hotKeyName } };

            _obs.SendRequest("TriggerHotkeyByName", requestFields);
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
