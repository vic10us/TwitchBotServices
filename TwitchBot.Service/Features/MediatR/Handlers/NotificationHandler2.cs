using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class NotificationHandler2 : INotificationHandler<NotificationMessage>
    {
        public Task Handle(NotificationMessage notification, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"Debugging from Notifier 2. Message  : {notification.NotifyText} ");
            return Task.CompletedTask;
        }
    }
}