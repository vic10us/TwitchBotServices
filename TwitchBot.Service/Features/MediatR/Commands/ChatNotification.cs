using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class ChatNotification : INotification
    {
        public ChatMessage ChatMessage { get; set; }

        public ChatNotification(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }
    }
}