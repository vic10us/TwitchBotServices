using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ganss.XSS;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Hubs;
using TwitchLib.Api;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class BasicChatNotificationHandler : INotificationHandler<BasicChatNotification>
    {
        private readonly HtmlSanitizer _sanitizer;
        private readonly TwitchAPI _twitchApiClient;
        private readonly IHubContext<TwitchHub> _twitchHub;

        public BasicChatNotificationHandler(HtmlSanitizer sanitizer, TwitchAPI twitchApiClient, IHubContext<TwitchHub> twitchHub)
        {
            _sanitizer = sanitizer;
            _twitchApiClient = twitchApiClient;
            _twitchHub = twitchHub;
        }

        public async Task Handle(BasicChatNotification notification, CancellationToken cancellationToken)
        {
            var uo = notification.User ?? _twitchApiClient.Helix.Users.GetUsersAsync(ids: new List<string> { notification.ChatMessageData.UserId }).Result.Users.FirstOrDefault();
            if (uo == null) return;
            notification.ChatMessageData.Message =
                System.Net.WebUtility.HtmlDecode(_sanitizer.Sanitize(notification.ChatMessageData.Message));
            notification.ChatMessageData.LogoUrl = uo.ProfileImageUrl;
            await _twitchHub.Clients.All.SendAsync("ReceiveChatMessage", notification.ChatMessageData, uo, cancellationToken: cancellationToken);
        }
    }
}