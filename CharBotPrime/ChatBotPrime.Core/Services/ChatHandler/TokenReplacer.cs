using System;
using System.Collections.Generic;
using Message = ChatBotPrime.Core.Events.EventArguments.ChatMessage;

namespace ChatBotPrime.Core.Services.CommandHandler
{
	public class TokenReplacer
	{
		public static TokenReplacer UserDisplayName =
			 new TokenReplacer(nameof(UserDisplayName), e => e.Username);

		protected TokenReplacer(string replacementString, Func<Message, string> replacementValueSelector)
		{
			_replacementString = replacementString;
			_replacementValueSelector = replacementValueSelector;
		}

		protected readonly string _replacementString;
		protected readonly Func<Message, string> _replacementValueSelector;

		public string ReplacementToken => $"[{_replacementString}]";

		public string ReplaceValues(string inputText, Message chatMessage)
		{
			return inputText.Replace(ReplacementToken, _replacementValueSelector(chatMessage));
		}

		public static readonly List<TokenReplacer> ListAll = new List<TokenReplacer>
		{
			UserDisplayName,
		};

	}
}
