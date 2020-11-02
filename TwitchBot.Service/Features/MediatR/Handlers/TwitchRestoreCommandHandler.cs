using System;
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
            var currentScenes = _obs.OBSClient.GetSceneList();
            if (!currentScenes.CurrentScene.StartsWith("Main", StringComparison.InvariantCultureIgnoreCase)) return Task.CompletedTask;
            _obs.OBSClient.SetCurrentScene("Main 050");
            return Task.CompletedTask;
        }
    }
}