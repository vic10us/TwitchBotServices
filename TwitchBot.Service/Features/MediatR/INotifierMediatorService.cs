using MediatR;

namespace TwitchBot.Service.Features.MediatR
{
    public interface INotifierMediatorService
    {
        void Notify(string notifyText);
        void Notify(INotification obj);
    }

    public class NotificationMessage : INotification
    {
        public string NotifyText { get; set; }
    }

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
