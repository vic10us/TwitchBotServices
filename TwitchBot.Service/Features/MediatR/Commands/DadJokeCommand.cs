using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class DadJokeCommand : INotification, IStringCommandMatcher, IChatCommand
    {
        private const string CommandIdentifier = "^dadjoke$";
        private static Regex Matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);
        public ChatCommand ChatCommand;

        public DadJokeCommand() { }

        public DadJokeCommand(ChatCommand command)
        {
            ChatCommand = command;
        }

        public bool Match(string commandName)
        {
            return Matcher.IsMatch(commandName);
        }
    }
}