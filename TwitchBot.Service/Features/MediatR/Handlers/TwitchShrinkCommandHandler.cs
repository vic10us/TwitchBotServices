using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class TwitchShrinkCommandHandler : INotificationHandler<ShrinkCommand>
    {
        private readonly OBSServices _obs;

        public TwitchShrinkCommandHandler(OBSServices obs)
        {
            _obs = obs;
        }

        public Task Handle(ShrinkCommand notification, CancellationToken cancellationToken)
        {
            var currentScenes = _obs.OBSClient.GetSceneList();
            if (!currentScenes.CurrentScene.StartsWith("Main", StringComparison.InvariantCultureIgnoreCase)) return Task.CompletedTask;
            var mainScenes = currentScenes.Scenes.Where(n => n.Name.StartsWith("Main", StringComparison.InvariantCultureIgnoreCase)).ToList();
            var i = mainScenes.FindIndex(0, s => s.Name.Equals(currentScenes.CurrentScene));
            if (i == 0) return Task.CompletedTask;
            _obs.OBSClient.SetCurrentScene(mainScenes[i - 1].Name);
            return Task.CompletedTask;
        }
    }
}