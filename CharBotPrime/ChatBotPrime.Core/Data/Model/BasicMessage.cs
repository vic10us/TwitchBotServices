using ChatBotPrime.Core.Events.EventArguments;
using ChatBotPrime.Core.Interfaces.Chat;
using ChatBotPrime.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Message = ChatBotPrime.Core.Events.EventArguments.ChatMessage;
using ChatBotPrime.Core.Services.CommandHandler;

namespace ChatBotPrime.Core.Data.Model
{
	public class BasicMessage : DataEntity, IChatMessage
	{
		public BasicMessage()
		{ }
		public BasicMessage(string messageText, string response, List<MessageAlias> aliases = null)
		{
			MessageText = messageText;
			Response = response;
			Aliases = aliases;
		}

		public string Response { get; private set; }
		public virtual ICollection<MessageAlias> Aliases { get; set; }

		public string MessageText {get; private set;}

		public bool IsMatch(string messageText)
		{

			return messageText.Contains(MessageText,StringComparison.InvariantCultureIgnoreCase) || Aliases.Where(a => messageText.Contains(a.Word,StringComparison.InvariantCultureIgnoreCase)).Any();
		}

		string IChatMessage.Response(IChatService service, Message chatMessage)
		{
			IEnumerable<string> findTokens = Response.FindTokens();
			string textToSend = ReplaceTokens(Response, findTokens, chatMessage);
			return textToSend;
		}

		private string ReplaceTokens(string textToSend, IEnumerable<string> tokens, Message chatMessage)
		{
			string newText = textToSend;
			var replaceableTokens = TokenReplacer.ListAll.Where(x => tokens.Contains(x.ReplacementToken));
			foreach (var rt in replaceableTokens)
			{
				newText = rt.ReplaceValues(newText, chatMessage);
			}

			return newText;
		}
	}
}
