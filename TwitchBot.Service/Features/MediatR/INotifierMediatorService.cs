using MediatR;

namespace TwitchBot.Service.Features.MediatR
{
    public interface INotifierMediatorService
    {
        void Notify(string notifyText);
        void Notify(INotification obj);
    }
}
