using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Models;
using TwitchBot.Service.Services;
using TwitchLib.Api;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchStatsCommandHandler : INotificationHandler<StatsCommand>
    {
        private readonly TwitchAPI _twitchApiClient;
        private readonly TwitchConfig _config;
        private readonly TwitchClientServices _twitchClientServices;

        public TwitchStatsCommandHandler(TwitchAPI twitchApiClient, IOptions<TwitchConfig> config, TwitchClientServices twitchClientServices)
        {
            _twitchApiClient = twitchApiClient;
            _twitchClientServices = twitchClientServices;
            _config = config.Value;
        }

        public async Task Handle(StatsCommand notification, CancellationToken cancellationToken)
        {
            var user = (await _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { "vic10usx" })).Users.FirstOrDefault();
            var channel = await _twitchApiClient.V5.Channels.GetChannelByIDAsync(user.Id);
            var streams = await _twitchApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { user.Id });
            var subs = await _twitchApiClient.Helix.Subscriptions.GetBroadcasterSubscriptions(user.Id);
            _twitchClientServices.Client.SendMessage(_config.Chat.Channel, $"Channel Views: {channel.Views}, Channel Followers: {channel.Followers}, Viewer Count: {streams.Streams.FirstOrDefault()?.ViewerCount ?? 0}, Subscriptions: {subs.Data.Count()}");
        }
    }
}