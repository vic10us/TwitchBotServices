using MediatR;

namespace TwitchBot.Service.Features.MediatR
{
    public class NotificationMessage : INotification
    {
        public string NotifyText { get; set; }
    }
}