using MediatR;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class CallOutCommand : INotification
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public CallOutCommand(string userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }
    }
}