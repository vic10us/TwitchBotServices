using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using AutoMapper;
using ChatBotPrime.Core.Interfaces.Chat;
using Ganss.XSS;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OBS.WebSockets.Core;
using TwitchBot.Service.Features.MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Hubs;
using TwitchBot.Service.Models;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub.Events;
using ChatMessage = ChatBotPrime.Core.Events.EventArguments.ChatMessage;

namespace TwitchBot.Service.Services
{
    public class TwitchBot
    {
        private readonly TwitchConfig _config;
        private readonly TwitchClient _client;
        private readonly ILogger<TwitchBot> _logger;
        private readonly TwitchAPI _twitchApiClient;
        private readonly HtmlSanitizer _sanitizer;
        private readonly IMapper _mapper;
        private readonly MemoryCache _cache;
        //private readonly OBSWebsocket _obs;
        //private readonly OBSConfig _obsConfig;
        private readonly WLEDService _wledService;
        private readonly IEnumerable<IChatCommand> _chatCommands;
        private readonly IHubContext<TwitchHub> _twitchHub;
        private readonly INotifierMediatorService _notifierMediatorService;

        public TwitchBot(
            IOptions<TwitchConfig> config,
            //IOptions<OBSConfig> obsConfig,
            ILogger<TwitchBot> logger,
            IHubContext<TwitchHub> twitchHub,
            HtmlSanitizer sanitizer,
            TwitchMemoryCache twitchMemoryCache,
            TwitchAPI twitchApi,
            IMapper mapper,
            //OBSWebsocket obs,
            WLEDService wledService,
            IEnumerable<IChatCommand> chatCommands, INotifierMediatorService notifierMediatorService)
        {
            _config = config.Value ?? throw new MissingConfigException();
            _twitchHub = twitchHub;
            _logger = logger;
            _sanitizer = sanitizer;
            _mapper = mapper;
            _cache = twitchMemoryCache.Cache;
            //_obs = obs;
            //_obsConfig = obsConfig.Value;
            _wledService = wledService;
            _chatCommands = chatCommands;
            _notifierMediatorService = notifierMediatorService;

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
            _client.OnDisconnected += ClientOnDisconnected;
            _client.Connect();

            // SetupOBS();
        }

        private void ClientOnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            if (!_client.IsConnected) _client.Connect();
        }

        private async void ClientOnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (
                (e.ChatMessage.Message.Trim().StartsWith("!")) ||
                (_config.IgnoredUsers.Any(iu => iu.Equals(e.ChatMessage.Username, StringComparison.InvariantCultureIgnoreCase)))
               ) return;

            await CallOutUser(e.ChatMessage);
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

            var uo = _twitchApiClient.Helix.Users.GetUsersAsync(ids: new List<string> { data.UserId }).Result.Users.FirstOrDefault();
            data.LogoUrl = uo?.ProfileImageUrl;
            await _twitchHub.Clients.All.SendAsync("ReceiveChatMessage", data, uo);
        }

        private async Task CallOutUser(TwitchLibMessage e)
        {
            var teamMember = _config.TeamMembers.FirstOrDefault(tm =>
                tm.Id.Equals(e.UserId, StringComparison.InvariantCultureIgnoreCase));

            if (teamMember != null && !teamMember.IgnoreShoutOut &&
                !_cache.TryGetValue($"TeamShoutout:{e.UserId}", out string cacheEntry))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                var channel = await _twitchApiClient.V5.Channels.GetChannelByIDAsync(e.UserId);
                var message = @$"{_config.TeamName} team member detected! 
    YEET!, @{e.Username}! 
    Check out their channel here: https://twitch.tv/{e.Username} 
    | They were last seen streaming {channel.Status} in {channel.Game}";
                _client.SendMessage(_config.Chat.Channel, message);
                _cache.Set($"TeamShoutout:{e.UserId}", cacheEntry, cacheEntryOptions);
            }
        }

        private void OnFollow(object sender, OnFollowArgs e)
        {
            _logger.LogInformation($"{JsonConvert.SerializeObject(e, Formatting.Indented)}");
        }

        private async void ClientOnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            _logger.LogInformation($"Received Chat Command: {JsonConvert.SerializeObject(e.Command, Formatting.Indented)}");
            var commandUser = (await _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { $"{e.Command.ChatMessage.UserId}" })).Users.FirstOrDefault();
            await CallOutUser(e.Command.ChatMessage);

            var cca = new ChatBotPrime.Core.Events.EventArguments.ChatCommand(e.Command.ArgumentsAsList, e.Command.ArgumentsAsString, e.Command.CommandIdentifier, e.Command.CommandText, _mapper.Map<ChatMessage>(e.Command.ChatMessage));
            var response = _chatCommands.FirstOrDefault(cc => cc.IsMatch(e.Command.CommandText));
            if (response != null) _client.SendMessage(_config.Chat.Channel, response.Response(null, cca));
            
            switch (e.Command.CommandText.ToLowerInvariant())
            {
                case "drop":
                    var uo = _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { e.Command.ChatMessage.DisplayName }).Result.Users.FirstOrDefault();
                    await _twitchHub.Clients.All.SendAsync("ReceiveMessage", e.Command.ChatMessage.DisplayName, "Make id rain", uo);
                    // await _twitchHub.Clients.All.SendAsync("SendMessage", e.Command.ChatMessage.DisplayName, "Make it rain!!!");
                    // await _hubConnection.InvokeAsync("SendMessage", e.Command.ChatMessage.DisplayName, "Make it rain!!!");
                    break;
                //case "yeet":
                //    var param = e.Command.ArgumentsAsList.Any() ? e.Command.ArgumentsAsString : "yourself";
                //    _client.SendMessage(_config.Chat.Channel, $"You yeeted {param} into tomorrow!");
                //    break;
                case "stats":
                    var user = (await _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> {"vic10usx"})).Users.FirstOrDefault();
                    var channel = await _twitchApiClient.V5.Channels.GetChannelByIDAsync(user.Id);
                    var streams = await _twitchApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> {user.Id});
                    var subs = await _twitchApiClient.Helix.Subscriptions.GetBroadcasterSubscriptions(user.Id);
                    _client.SendMessage(_config.Chat.Channel, $"Channel Views: {channel.Views}, Channel Followers: {channel.Followers}, Viewer Count: {streams.Streams.FirstOrDefault()?.ViewerCount ?? 0}, Subscriptions: {subs.Data.Count()}");
                    break;
                case "grow":
                    _notifierMediatorService.Notify(new GrowCommand());
                    break;
                case "shrink":
                    _notifierMediatorService.Notify(new ShrinkCommand());
                    break;
                case "restore":
                    _notifierMediatorService.Notify(new RestoreCommand());
                    break;
                case "ledfx":
                    if (string.IsNullOrWhiteSpace(e.Command.ArgumentsAsString))
                    {
                        _client.SendMessage(_config.Chat.Channel, "ledfx command requires the name of the fx you want to set. see reference below?");
                        break;
                    }
                    var conf = await _wledService.GetRootConfig();
                    var fxItem = conf.effects.ToList().FindIndex(c => c.Equals(e.Command.ArgumentsAsString, StringComparison.InvariantCultureIgnoreCase));
                    if (fxItem >= 0)
                    {
                        await _wledService.SetFx(fxItem);
                    }
                    else
                    {
                        _client.SendMessage(_config.Chat.Channel, $"\"{e.Command.ArgumentsAsString}\" is not a valid FX");
                    }
                    // _client.SendWhisper(e.Command.ChatMessage.Username, message);
                    // _client.SendMessage(_config.Chat.Channel, message);
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
        /// <param name="hotKeyName">The desired scene name</param>
        //public void TriggerHotKeyByName(string hotKeyName)
        //{
        //    var requestFields = new JObject { { "hotkeyName", hotKeyName } };

        //    _obs.SendRequest("TriggerHotkeyByName", requestFields);
        //}

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
