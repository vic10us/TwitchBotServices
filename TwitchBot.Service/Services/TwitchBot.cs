using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ganss.XSS;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TwitchBot.Service.Features.MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Hubs;
using TwitchBot.Service.Models;
using TwitchLib.Api;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Services
{
    public class TwitchBot
    {
        private readonly TwitchConfig _config;
        private readonly ILogger<TwitchBot> _logger;
        private readonly TwitchAPI _twitchApiClient;
        private readonly HtmlSanitizer _sanitizer;
        private readonly IMapper _mapper;
        private readonly MemoryCache _cache;
        private readonly WLEDService _wledService;
        private readonly IHubContext<TwitchHub> _twitchHub;
        private readonly INotifierMediatorService _notifierMediatorService;
        private readonly TwitchPubSub _pubSubClient;
        private readonly TwitchClientServices _twitchClientServices;

        public TwitchBot(
            IOptions<TwitchConfig> config,
            ILogger<TwitchBot> logger,
            IHubContext<TwitchHub> twitchHub,
            HtmlSanitizer sanitizer,
            TwitchMemoryCache twitchMemoryCache,
            TwitchAPI twitchApi,
            IMapper mapper,
            WLEDService wledService,
            TwitchClientServices twitchClientServices,
            INotifierMediatorService notifierMediatorService)
        {
            _config = config.Value ?? throw new MissingConfigException();
            _twitchHub = twitchHub;
            _logger = logger;
            _sanitizer = sanitizer;
            _mapper = mapper;
            _cache = twitchMemoryCache.Cache;
            _wledService = wledService;
            _notifierMediatorService = notifierMediatorService;
            _twitchClientServices = twitchClientServices;

            _twitchApiClient = twitchApi;

            _pubSubClient = new TwitchPubSub();
            var user = _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> {"vic10usx"}).Result.Users.First();
            _pubSubClient.OnPubSubServiceConnected += OnPubSubServiceConnected;
            _pubSubClient.ListenToRewards(user.Id);
            _pubSubClient.OnRewardRedeemed += OnRewardRedeemed;
            _pubSubClient.Connect();

            _twitchClientServices.Client.OnChatCommandReceived += ClientOnChatCommandReceived;
            _twitchClientServices.Client.OnMessageReceived += ClientOnMessageReceived;
        }

        private void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            // SendTopics accepts an oauth optionally, which is necessary for some topics
            _pubSubClient.SendTopics(_config.Chat.PasswordGeneratorToken);
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            var x = _twitchApiClient.Helix.Entitlements.GetCodeStatusAsync(new List<string> { $"{e.RewardId}" }, _config.Chat.PasswordGeneratorToken).Result;
            _twitchClientServices.Client.SendMessage(_config.Chat.Channel, $"Redeemed: {e.RewardId} {x.Data.FirstOrDefault()?.StatusEnum}");
        }

        private async void ClientOnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (
                (e.ChatMessage.Message.Trim().StartsWith("!")) ||
                (_config.IgnoredUsers.Any(iu => iu.Equals(e.ChatMessage.Username, StringComparison.InvariantCultureIgnoreCase)))
               ) return;

            await CallOutUser(e.ChatMessage);
            var sanitizedHtml = System.Net.WebUtility.HtmlDecode(_sanitizer.Sanitize(e.ChatMessage.Message));
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
                _twitchClientServices.Client.SendMessage(_config.Chat.Channel, message);
                _cache.Set($"TeamShoutout:{e.UserId}", cacheEntry, cacheEntryOptions);
            }
        }

        private async void ClientOnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            _logger.LogInformation($"Received Chat Command: {JsonConvert.SerializeObject(e.Command, Formatting.Indented)}");
            await CallOutUser(e.Command.ChatMessage);
            
            switch (e.Command.CommandText.ToLowerInvariant())
            {
                case "drop":
                    var uo = _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { e.Command.ChatMessage.DisplayName }).Result.Users.FirstOrDefault();
                    await _twitchHub.Clients.All.SendAsync("ReceiveMessage", e.Command.ChatMessage.DisplayName, "Make id rain", uo);
                    break;
                case "yeet":
                    _notifierMediatorService.Notify(new YeetCommand(e.Command));
                    break;
                case "stats":
                    var user = (await _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> {"vic10usx"})).Users.FirstOrDefault();
                    var channel = await _twitchApiClient.V5.Channels.GetChannelByIDAsync(user.Id);
                    var streams = await _twitchApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> {user.Id});
                    var subs = await _twitchApiClient.Helix.Subscriptions.GetBroadcasterSubscriptions(user.Id);
                    _twitchClientServices.Client.SendMessage(_config.Chat.Channel, $"Channel Views: {channel.Views}, Channel Followers: {channel.Followers}, Viewer Count: {streams.Streams.FirstOrDefault()?.ViewerCount ?? 0}, Subscriptions: {subs.Data.Count()}");
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
                        _twitchClientServices.Client.SendMessage(_config.Chat.Channel, "ledfx command requires the name of the fx you want to set. see reference below?");
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
                        _twitchClientServices.Client.SendMessage(_config.Chat.Channel, $"\"{e.Command.ArgumentsAsString}\" is not a valid FX");
                    }
                    break;
                default:
                    if (_config.Chat.RespondToUnknownCommand)
                        _twitchClientServices.Client.SendMessage(
                            _config.Chat.Channel, 
                            $"y33t you! {e.Command.CommandText} obvs doesn't exist! n00b!"
                        );
                    break;
            }
        }
    }
}
