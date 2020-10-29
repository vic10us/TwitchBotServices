using ChatBotPrime.Core.Interfaces.Chat;
using ChatBotPrime.Core.Services.CommandHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using Command = ChatBotPrime.Core.Events.EventArguments.ChatCommand;
using ChatBotPrime.Core.Extensions;

namespace ChatBotPrime.Core.Data.Model
{
	public class BasicCommand	: DataEntity , IChatCommand
	{
		public BasicCommand()
		{ }
		public BasicCommand(string commandText, string response, bool isEnabled = true  )
		{
			CommandText = commandText;
			Response = response;
			IsEnabled = isEnabled;
		}
			
		public TimeSpan Cooldown { get; set; } = TimeSpan.Zero;
		public virtual ICollection<CommandAlias> Aliases { get; set; }

		public string CommandText { get; private set; }

		public bool IsEnabled { get; private set; }

		public DateTime LastRun { get; set; } = DateTime.UtcNow;

		public bool IsMatch(string command)
		{
			if (IsEnabled)
			{
				return command.Equals(CommandText, StringComparison.InvariantCultureIgnoreCase) || Aliases.Where(a => a.Word.Equals(command, StringComparison.InvariantCultureIgnoreCase)).Any();
			}
			return false;
		}

		public string Response { get; private set; }

		string IChatCommand.Response(IChatService service,Command chatMessage)
		{
			if (CanRun())
			{
				IEnumerable<string> findTokens = Response.FindTokens();
				string textToSend = ReplaceTokens(Response, findTokens, chatMessage);
				SetLastRun();
				return textToSend;
			}
			return $"Command is on cooldown please wait {GetTimeToRun()}";
		}

		private string ReplaceTokens(string textToSend, IEnumerable<string> tokens, Command cahtCommand)
		{
			string newText = textToSend;
			var replaceableTokens = TokenReplacer.ListAll.Where(x => tokens.Contains(x.ReplacementToken));
			foreach (var rt in replaceableTokens)
			{
				newText = rt.ReplaceValues(newText, cahtCommand.ChatMessage);
			}

			return newText;
		}

		private   string GetTimeToRun()
		{
			var time = (Cooldown - (DateTime.UtcNow - LastRun));
			if (time.Minutes > 0)
			{
				return $"{time.Minutes}M{time.Seconds}S";
			}
			 else
			{
				return $"{time.Seconds}S";
			}

		}

		private   bool CanRun()
		{
			if (Cooldown == TimeSpan.Zero)
				return true;

			return DateTime.UtcNow - LastRun >= Cooldown;
		}

		private  void SetLastRun()
		{
			LastRun = DateTime.UtcNow;
		}
	}
}
