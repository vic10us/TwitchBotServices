using System.Text.RegularExpressions;
using MediatR;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class ShrinkCommand : INotification, IStringCommandMatcher, INullCommand
    {
        private const string CommandIdentifier = "^shrink[!]*$";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);
        
        public ShrinkCommand() { }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}