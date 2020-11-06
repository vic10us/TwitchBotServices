using System.Text.RegularExpressions;
using MediatR;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class StatsCommand : INotification, IStringCommandMatcher, INullCommand
    {
        private const string CommandIdentifier = "^stats$";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public StatsCommand() { }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}