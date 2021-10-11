using System.Linq;
using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class CallOutCommand : 
        INotification, 
        IStringCommandMatcher, 
        IStringParamCommand, 
        IChatCommand, 
        IChatMessageCommand
    {
        private const string CommandIdentifier = "^(callout|co|so|shoutout)$";
        private static Regex Matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public string UserId { get; set; } = null;
        public string UserName { get; set; }

        public CallOutCommand() { }

        public CallOutCommand(string userName)
        {
            this.UserName = userName;
        }

        public CallOutCommand(ChatCommand command)
        {
            if (command.ArgumentsAsList.Any())
            {
                UserId = null;
                UserName = command.ArgumentsAsString;
            }
            else
            {
                UserId = command.ChatMessage.UserId;
                UserName = command.ChatMessage.Username;
            }
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
            return Matcher.IsMatch(commandName);
        }
    }
}