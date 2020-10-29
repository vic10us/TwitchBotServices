using ChatBotPrime.Core.Services.CommandHandler;
using System;
using System.Linq;
using ChatBotPrime.Core.Interfaces.Chat;
using TwitchLib.Api;
using ChatCommand = ChatBotPrime.Core.Events.EventArguments.ChatCommand;

namespace TwitchBot.Service.ChatCommands
{
    public class TwitchYeetCommand : StreamCommand
    {
        public override string CommandText => "yeet";
        
        public override string Response(IChatService streamService, ChatCommand chatCommand)
        {
            // if (!CanRun()) return $"Command is on cooldown please wait {GetTimeToRun()}";

            // SetLastRun();
            var param = chatCommand.ArgumentsAsList.Any() ? chatCommand.ArgumentsAsString : chatCommand.ChatMessage.Username;
            return $"You yeeted {param} into tomorrow!";
        }

        public override TimeSpan Cooldown {
            get => TimeSpan.FromMilliseconds(250);
            set { }
        }
    }
}
