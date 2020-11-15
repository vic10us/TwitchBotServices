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
    public class TwitchWeatherCommandHandler : INotificationHandler<WeatherCommand>
    {
        private readonly TwitchAPI _twitchApiClient;
        private readonly IHubContext<TwitchHub> _twitchHub;

        public TwitchWeatherCommandHandler(TwitchAPI twitchApiClient, IHubContext<TwitchHub> twitchHub)
        {
            _twitchApiClient = twitchApiClient;
            _twitchHub = twitchHub;
        }

        public async Task Handle(WeatherCommand notification, CancellationToken cancellationToken)
        {
            var uo = _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { notification.Command.ChatMessage.DisplayName }).Result.Users.FirstOrDefault();
            var data = new
            {
                user = uo,
                weatherEvent = $"!{notification.Command.CommandText}"
            };
            await _twitchHub.Clients.All.SendAsync("ReceiveMessage", notification.Command.ChatMessage.DisplayName, "weather", data, cancellationToken: cancellationToken);
        }
    }
}