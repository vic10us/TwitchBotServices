using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class ChatNotification : INotification, IChatCommand, IChatMessageCommand
    {
        public ChatMessage ChatMessage { get; set; }

        public ChatNotification() { }

        public ChatNotification(ChatCommand command)
        {
            ChatMessage = command.ChatMessage;
        }

        public ChatNotification(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }
    }
}