using System;
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
    public class TwitchCallOutCommandHandler : INotificationHandler<CallOutCommand>
    {
        private readonly TwitchConfig _config;
        private readonly ICacheService _cacheService;
        private readonly TwitchAPI _twitchApiClient;
        private readonly TwitchClientServices _twitchClientServices;

        public TwitchCallOutCommandHandler(TwitchClientServices twitchClientServices, TwitchAPI twitchApiClient, ICacheService cacheService, IOptions<TwitchConfig> config)
        {
            _twitchClientServices = twitchClientServices;
            _twitchApiClient = twitchApiClient;
            _cacheService = cacheService;
            _config = config.Value;
        }

        public async Task Handle(CallOutCommand notification, CancellationToken cancellationToken)
        {
            await CallOutUser(notification.UserId, notification.UserName);
        }

        private async Task CallOutUser(string userId, string userName)
        {
            var teamMember = _config.TeamMembers.FirstOrDefault(tm =>
                tm.Id.Equals(userId, StringComparison.InvariantCultureIgnoreCase));

            if (teamMember == null || teamMember.IgnoreShoutOut) return;

            var cacheKey = $"TeamShoutout:{userId}";
            if (await _cacheService.ValueExists(cacheKey)) return; // Team Member shoutout already done. :)

            // Add the teamMember to the cache.
            var _ = await _cacheService.GetOrSetValue(cacheKey, () => Task.FromResult(teamMember), new TwitchCacheOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30),
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)
            });

            var channel = await _twitchApiClient.V5.Channels.GetChannelByIDAsync(userId);
            var message = @$"{_config.TeamName} team member detected! 
    YEET!, @{userName}! 
    Check out their channel here: https://twitch.tv/{userName} 
    | They were last seen streaming {channel.Status} in {channel.Game}";
            _twitchClientServices.Client.SendMessage(_config.Chat.Channel, message);
        }
    }
}