using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class YeetCommand : INotification
    {
        public ChatCommand ChatCommand { get; set; }

        public YeetCommand() { }

        public YeetCommand(ChatCommand chatCommand)
        {
            ChatCommand = chatCommand;
        }
    }
}