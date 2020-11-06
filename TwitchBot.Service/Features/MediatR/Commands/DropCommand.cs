using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class DropCommand : INotification, IStringCommandMatcher, IChatCommand, IChatMessageCommand
    {
        private const string CommandIdentifier = "drop";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public ChatMessage ChatMessage { get; set; }
        
        public DropCommand() { }

        public DropCommand(ChatCommand command)
        {
            ChatMessage = command.ChatMessage;
        }

        public DropCommand(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}