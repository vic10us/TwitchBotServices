using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class DropCommand : INotification
    {
        public ChatMessage ChatMessage { get; set; }

        public DropCommand(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }
    }
}