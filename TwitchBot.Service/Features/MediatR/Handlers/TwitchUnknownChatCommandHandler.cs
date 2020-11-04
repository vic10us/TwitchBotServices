using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Models;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchUnknownChatCommandHandler : INotificationHandler<UnknownChatCommand>
    {
        private readonly TwitchConfig _config;
        private readonly TwitchClientServices _twitchClientServices;

        public TwitchUnknownChatCommandHandler(IOptions<TwitchConfig> config, TwitchClientServices twitchClientServices)
        {
            _twitchClientServices = twitchClientServices;
            _config = config.Value;
        }

        public Task Handle(UnknownChatCommand notification, CancellationToken cancellationToken)
        {
            if (_config.Chat.RespondToUnknownCommand)
                _twitchClientServices.Client.SendMessage(
                    _config.Chat.Channel,
                    $"y33t you! {notification.Command} obvs doesn't exist! n00b!"
                );
            return Task.CompletedTask;
        }
    }
}