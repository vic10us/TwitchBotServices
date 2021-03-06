﻿using System.Text.RegularExpressions;
using MediatR;

namespace TwitchBot.Service.Features.MediatR.Commands
{
    public class RestoreCommand : INotification, IStringCommandMatcher, INullCommand
    {
        private const string CommandIdentifier = "restore";
        private static Regex matcher => new Regex(CommandIdentifier, RegexOptions.IgnoreCase);

        public RestoreCommand() { }

        public bool Match(string commandName)
        {
            return matcher.IsMatch(commandName);
        }
    }
}