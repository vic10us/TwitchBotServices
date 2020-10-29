using ChatBotPrime.Core.Events.EventArguments;
using ChatBotPrime.Core.Interfaces.Chat;
using System;
using Message = ChatBotPrime.Core.Events.EventArguments.ChatMessage;

namespace ChatBotPrime.Core.Services.CommandHandler
{
	public abstract class ChatMessage : IChatMessage
	{
		public abstract string MessageText { get; }

		public virtual bool IsMatch(string messageText)
		{
			return messageText.Contains(MessageText);
		}
		
		public abstract string Response(IChatService service, Message message);
	}
}
