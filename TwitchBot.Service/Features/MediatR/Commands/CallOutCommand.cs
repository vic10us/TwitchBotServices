using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class CallOutCommand : INotification, IStringCommandMatcher, IChatCommand, IChatMessageCommand
    {
        private const string CommandIdentifier = "^(callout|co)$";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public string UserId { get; set; }
        public string UserName { get; set; }

        public CallOutCommand() { }

        public CallOutCommand(ChatCommand command)
        {
            UserId = command.ChatMessage.UserId;
            UserName = command.ChatMessage.Username;
        }

        public CallOutCommand(ChatMessage message)
        {
            UserId = message.UserId;
            UserName = message.Username;
        }

        public CallOutCommand(string userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}