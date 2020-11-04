using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchGrowCommandHandler : INotificationHandler<GrowCommand>
    {
        private readonly OBSServices _obs;

        public TwitchGrowCommandHandler(OBSServices obs)
        {
            _obs = obs;
        }

        public Task Handle(GrowCommand notification, CancellationToken cancellationToken)
        {
            var i = _obs.OBSClient
                .GetSourceFilters("Main")
                .Where(sf => sf.IsEnabled && sf.Type.Equals("move_source_filter", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            if (!i.Any()) return Task.CompletedTask;
            var active = 
                i.FirstOrDefault(sf => sf.Settings["activated"] != null && (bool)sf.Settings["activated"])
                         ?? i.FirstOrDefault(sf => sf.Name.Equals("Main075")) 
                         ?? i.First();
            if (active == null) return Task.CompletedTask;
            var nextIndex = i.IndexOf(active) + 1;
            if (nextIndex > i.Count-1) return Task.CompletedTask;
            var next = i[nextIndex];
            _obs.TriggerHotKeyByName(next.Name);
            for (var p = 0; p < i.Count; p++)
            {
                var current = i[p];
                current.Settings["activated"] = p == nextIndex;
                _obs.OBSClient.SetSourceFilterSettings("Main", current.Name, current.Settings);
            }
            return Task.CompletedTask;
        }
    }
}