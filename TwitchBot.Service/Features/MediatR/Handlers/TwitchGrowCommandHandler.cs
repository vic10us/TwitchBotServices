using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Services;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchGrowCommandHandler : INotificationHandler<GrowCommand>
    {
        private readonly OBSServices _obs;
        private readonly TwitchClientServices _api;

        public TwitchGrowCommandHandler(OBSServices obs, TwitchClientServices api)
        {
            _obs = obs;
            _api = api;
        }

        public async Task Handle(GrowCommand notification, CancellationToken cancellationToken)
        {
            var e = notification.RewardRedeemedArgs;
            var i = _obs.OBSClient
                .GetSourceFilters("Main")
                .Where(sf => sf.IsEnabled && sf.Type.Equals("move_source_filter", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            if (!i.Any())
            {
                await Cancel(e);
                return;
            }
            var active = 
                i.FirstOrDefault(sf => sf.Settings["activated"] != null && (bool)sf.Settings["activated"])
                         ?? i.FirstOrDefault(sf => sf.Name.Equals("Main075")) 
                         ?? i.First();
            if (active == null)
            {
                await Cancel(e);
                return;
            }
            var nextIndex = i.IndexOf(active) + 1;
            if (nextIndex > i.Count-1)
            {
                await Cancel(e);
                return;
            }
            var next = i[nextIndex];
            _obs.TriggerHotKeyByName(next.Name);
            await _api.UpdateChannelRedemptionStatus($"{e.RedemptionId:D}", $"{e.RewardId:D}", RedemptionStatus.FULFILLED);
            for (var p = 0; p < i.Count; p++)
            {
                var current = i[p];
                current.Settings["activated"] = p == nextIndex;
                _obs.OBSClient.SetSourceFilterSettings("Main", current.Name, current.Settings);
            }
        }

        private async Task Cancel(OnRewardRedeemedArgs e)
        {
            await _api.UpdateChannelRedemptionStatus($"{e.RedemptionId:D}", $"{e.RewardId:D}", RedemptionStatus.CANCELED);
        }
    }
}