using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ganss.XSS;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Hubs;
using TwitchBot.Service.Models;
using TwitchLib.Api;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchChatNotificationHandler : INotificationHandler<ChatNotification>
    {
        private readonly IMapper _mapper;
        private readonly TwitchConfig _config;
        private readonly HtmlSanitizer _sanitizer;
        private readonly TwitchAPI _twitchApiClient;
        private readonly IHubContext<TwitchHub> _twitchHub;

        public TwitchChatNotificationHandler(IMapper mapper, IOptions<TwitchConfig> config, HtmlSanitizer sanitizer, TwitchAPI twitchApiClient, IHubContext<TwitchHub> twitchHub)
        {
            _mapper = mapper;
            _sanitizer = sanitizer;
            _twitchApiClient = twitchApiClient;
            _twitchHub = twitchHub;
            _config = config.Value;
        }

        public async Task Handle(ChatNotification notification, CancellationToken cancellationToken)
        {
            if (
                notification.ChatMessage.Message.Trim().StartsWith("!") ||
                _config.IgnoredUsers.Any(iu => iu.Equals(notification.ChatMessage.Username, StringComparison.InvariantCultureIgnoreCase))
            ) return;

            var sanitizedHtml = System.Net.WebUtility.HtmlDecode(_sanitizer.Sanitize(notification.ChatMessage.Message));
            var emotes = _mapper.Map<IEnumerable<EmoteDto>>(notification.ChatMessage.EmoteSet.Emotes);

            var userTypes = UserTypes.None;
            if (notification.ChatMessage.IsBroadcaster) userTypes |= UserTypes.Broadcaster;
            if (notification.ChatMessage.IsModerator) userTypes |= UserTypes.Moderator;
            if (notification.ChatMessage.IsSubscriber) userTypes |= UserTypes.Subscriber;
            if (notification.ChatMessage.IsVip) userTypes |= UserTypes.Vip;
            if (_config.TeamMembers.Any(tm =>
                tm.Id.Equals(notification.ChatMessage.UserId, StringComparison.InvariantCultureIgnoreCase)))
                userTypes |= UserTypes.TeamMember;

            var data = new ChatMessageData
            {
                MessageId = notification.ChatMessage.Id,
                UserId = notification.ChatMessage.UserId,
                UserName = notification.ChatMessage.Username,
                DisplayName = notification.ChatMessage.DisplayName,
                Message = sanitizedHtml,
                TeamName = _config.TeamName,
                TeamShoutoutEnabled = _config.TeamShoutoutEnabled,
                EmoteDetails = emotes.ToArray(),
                UserTypes = userTypes
            };

            var uo = _twitchApiClient.Helix.Users.GetUsersAsync(ids: new List<string> { data.UserId }).Result.Users.FirstOrDefault();
            data.LogoUrl = uo?.ProfileImageUrl;
            await _twitchHub.Clients.All.SendAsync("ReceiveChatMessage", data, uo, cancellationToken: cancellationToken);
        }
    }

    public class TwitchChatNotificationHandler2 : INotificationHandler<ChatNotification>
    {
        private readonly TwitchConfig _config;
        private readonly INotifierMediatorService _notifierMediatorService;

        public TwitchChatNotificationHandler2(IOptions<TwitchConfig> config, INotifierMediatorService notifierMediatorService)
        {
            _notifierMediatorService = notifierMediatorService;
            _config = config.Value;
        }

        public Task Handle(ChatNotification notification, CancellationToken cancellationToken)
        {
            if (
                notification.ChatMessage.Message.Trim().StartsWith("!") ||
                _config.IgnoredUsers.Any(iu => iu.Equals(notification.ChatMessage.Username, StringComparison.InvariantCultureIgnoreCase))
            ) return Task.CompletedTask;
            
            _notifierMediatorService.Notify(new CallOutCommand(notification.ChatMessage.UserId, notification.ChatMessage.Username));
            return Task.CompletedTask;
        }
    }
}