using System.Text.RegularExpressions;
using MediatR;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class ShrinkCommand : INotification, IStringCommandMatcher, IRedemptionCommand
    {
        private const string CommandIdentifier = "^shrink!$";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);
        public OnRewardRedeemedArgs RewardRedeemedArgs { get; set; }

        public ShrinkCommand() { }

        public ShrinkCommand(OnRewardRedeemedArgs rewardRedeemedArgs)
        {
            RewardRedeemedArgs = rewardRedeemedArgs;
        }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}