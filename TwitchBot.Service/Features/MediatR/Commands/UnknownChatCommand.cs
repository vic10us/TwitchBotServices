using MediatR;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class UnknownChatCommand : INotification
    {
        public string Command { get; set; }

        public UnknownChatCommand(string command)
        {
            Command = command;
        }
    }
}