using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class LedFXCommand : INotification, IStringCommandMatcher, IChatCommand
    {
        private const string CommandIdentifier = "ledfx";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public string FX { get; set; }

        public LedFXCommand() { }

        public LedFXCommand(ChatCommand chatCommand)
        {
            FX = chatCommand.ArgumentsAsString;
        }

        public LedFXCommand(string fx)
        {
            FX = fx;
        }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}