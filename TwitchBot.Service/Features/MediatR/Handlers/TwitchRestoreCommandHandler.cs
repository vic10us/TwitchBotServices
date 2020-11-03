using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchRestoreCommandHandler : INotificationHandler<RestoreCommand>
    {
        private readonly OBSServices _obs;

        public TwitchRestoreCommandHandler(OBSServices obs)
        {
            _obs = obs;
        }

        public Task Handle(RestoreCommand notification, CancellationToken cancellationToken)
        {
            var i = _obs.OBSClient
                .GetSourceFilters("Main")
                .Where(sf => sf.IsEnabled && sf.Type.Equals("move_source_filter", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            if (!i.Any()) return Task.CompletedTask;
            var active = i.FirstOrDefault(sf => sf.Name.Equals("Main075"))
                                    ?? i.First();
            if (active == null) return Task.CompletedTask;
            _obs.TriggerHotKeyByName(active.Name);
            foreach (var current in i)
            {
                current.Settings["activated"] = current.Name.Equals(active.Name);
                _obs.OBSClient.SetSourceFilterSettings("Main", current.Name, current.Settings);
            }
            return Task.CompletedTask;
        }
    }
}