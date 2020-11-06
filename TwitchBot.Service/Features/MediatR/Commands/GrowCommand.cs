using System.Text.RegularExpressions;
using MediatR;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class GrowCommand : INotification, IStringCommandMatcher, INullCommand
    {
        private const string CommandIdentifier = "^grow[!]*$";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public GrowCommand() { }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}
