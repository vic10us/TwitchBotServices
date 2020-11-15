using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class WeatherCommand : INotification, IStringCommandMatcher, IChatCommand
    {
        private const string CommandIdentifier = "^(rain|snow|blizzard|hail|shower)$";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public ChatCommand Command { get; set; }

        public WeatherCommand() { }

        public WeatherCommand(ChatCommand command)
        {
            Command = command;
        }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}