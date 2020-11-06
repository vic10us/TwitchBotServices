using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class UnknownChatCommand : INotification, INullCommand
    {
        public string Command { get; set; }

        public UnknownChatCommand() { }

        public UnknownChatCommand(ChatCommand command)
        {
            Command = command.CommandText;
        }

        public UnknownChatCommand(string command)
        {
            Command = command;
        }
    }
}