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
            var user = (await _twitchApiClient.Helix.Users.GetUsersAsync(logins: ["thisistherealvoice"])).Users.FirstOrDefault();
            //var channels = await _twitchApiClient.Helix.Channels.GetChannelInformationAsync(user.Id);
            //var channel = channels.Data.FirstOrDefault();
            var channelFollowers = await _twitchApiClient.Helix.Channels.GetChannelFollowersAsync(user.Id);
            var streams = await _twitchApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { user.Id });
            var lastStream = streams.Streams.FirstOrDefault();
            var subscribers = await _twitchApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(user.Id);
            _twitchClientServices.Client.SendMessage(_config.Chat.Channel, $"Channel Views: {lastStream.ViewerCount}, Channel Followers: {channelFollowers.Total}, Viewer Count: {streams.Streams.FirstOrDefault()?.ViewerCount ?? 0}, Subscriptions: {subscribers.Total}");
        }
    }
}