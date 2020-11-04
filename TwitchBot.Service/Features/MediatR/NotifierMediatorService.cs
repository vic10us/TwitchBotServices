using MediatR;

namespace TwitchBot.Service.Features.MediatR
{
    public class NotifierMediatorService : INotifierMediatorService
    {
        private readonly IMediator _mediator;

        public NotifierMediatorService(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public void Notify(string notifyText)
        {
            _mediator.Publish(new NotificationMessage { NotifyText = notifyText });
        }

        public void Notify(INotification obj)
        {
            _mediator.Publish(obj);
        }
    }
}