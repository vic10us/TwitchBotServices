using MediatR;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class LedFXCommand : INotification
    {
        public string FX { get; set; }

        public LedFXCommand(string fx)
        {
            FX = fx;
        }
    }
}