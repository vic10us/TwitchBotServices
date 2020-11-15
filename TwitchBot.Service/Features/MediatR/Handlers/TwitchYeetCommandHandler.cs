using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Models;
using TwitchBot.Service.Services;
using TwitchLib.Client;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchYeetCommandHandler : INotificationHandler<YeetCommand>
    {
        private readonly TwitchClient _twitchClient;
        private readonly TwitchConfig _config;

        public TwitchYeetCommandHandler(TwitchClientServices twitchClient, IOptions<TwitchConfig> config)
        {
            _config = config.Value;
            _twitchClient = twitchClient.Client;
        }

        public Task Handle(YeetCommand notification, CancellationToken cancellationToken)
        {
            var param = notification.ChatCommand.ArgumentsAsList.Any() ? notification.ChatCommand.ArgumentsAsString : notification.ChatCommand.ChatMessage.Username;
            var message = $"You yeeted {param} into tomorrow!";
            _twitchClient.SendMessage(_config.Chat.Channel, message);
            return Task.CompletedTask;
        }
    }
}