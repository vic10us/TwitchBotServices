using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Hubs;
using TwitchLib.Api;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchDropCommandHandler : INotificationHandler<DropCommand>
    {
        private readonly TwitchAPI _twitchApiClient;
        private readonly IHubContext<TwitchHub> _twitchHub;

        public TwitchDropCommandHandler(TwitchAPI twitchApiClient, IHubContext<TwitchHub> twitchHub)
        {
            _twitchApiClient = twitchApiClient;
            _twitchHub = twitchHub;
        }

        public async Task Handle(DropCommand notification, CancellationToken cancellationToken)
        {
            var uo = _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { notification.ChatMessage.DisplayName }).Result.Users.FirstOrDefault();
            await _twitchHub.Clients.All.SendAsync("ReceiveMessage", notification.ChatMessage.DisplayName, "Make it rain", uo, cancellationToken: cancellationToken);
        }
    }
}