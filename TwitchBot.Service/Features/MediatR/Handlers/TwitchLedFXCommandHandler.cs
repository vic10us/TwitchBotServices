using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Models;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchLedFXCommandHandler : INotificationHandler<LedFXCommand>
    {
        private readonly TwitchConfig _config;
        private readonly TwitchClientServices _twitchClientServices;
        private readonly WLEDService _wledService;

        public TwitchLedFXCommandHandler(WLEDService wledService, TwitchClientServices twitchClientServices, IOptions<TwitchConfig> config)
        {
            _wledService = wledService;
            _twitchClientServices = twitchClientServices;
            _config = config.Value;
        }

        public async Task Handle(LedFXCommand notification, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(notification.FX))
            {
                _twitchClientServices.Client.SendMessage(_config.Chat.Channel, "ledfx command requires the name of the fx you want to set. see reference below?");
                return;
            }
            var conf = await _wledService.GetRootConfig();
            var fxItem = conf.effects.ToList().FindIndex(c => c.Equals(notification.FX, StringComparison.InvariantCultureIgnoreCase));
            if (fxItem >= 0)
            {
                await _wledService.SetFx(fxItem);
            }
            else
            {
                _twitchClientServices.Client.SendMessage(_config.Chat.Channel, $"\"{notification.FX}\" is not a valid FX");
            }
        }
    }
}