using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class GrowCommand : INotification, IStringCommandMatcher, IRedemptionCommand
    {
        private const string CommandIdentifier = "^grow!$";
        private static Regex Matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);
        public OnRewardRedeemedArgs RewardRedeemedArgs { get; set; }

        public GrowCommand() { }

        public GrowCommand(OnRewardRedeemedArgs rewardRedeemedArgs)
        {
            RewardRedeemedArgs = rewardRedeemedArgs;
        }

        public bool Match(string commandName)
        {
            return Matcher.IsMatch(commandName);
        }
    }
}
