using MediatR;
using TwitchBot.Service.Models;
using TwitchLib.Api.Helix.Models.Users;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class BasicChatNotification : INotification
    {
        public ChatMessageData ChatMessageData;
        public User User;

        public BasicChatNotification() { }

        public BasicChatNotification(ChatMessageData data)
        {
            ChatMessageData = data;
            User = null;
        }

        public BasicChatNotification(ChatMessageData data, User u)
        {
            ChatMessageData = data;
            User = u;
        }
    }
}