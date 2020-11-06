using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class YeetCommand : INotification, IStringCommandMatcher, IChatCommand
    {
        private const string CommandIdentifier = "yeet";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public ChatCommand ChatCommand { get; set; }

        public YeetCommand() { }

        public YeetCommand(ChatCommand chatCommand)
        {
            ChatCommand = chatCommand;
        }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}