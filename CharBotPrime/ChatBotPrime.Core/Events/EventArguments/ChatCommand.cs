using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotPrime.Core.Events.EventArguments
{
	public class ChatCommand
	{
		public ChatCommand(List<string> argumentsAsList, string argumentsAsString, char commandIdentifier, string commandText, ChatMessage chatMessage)
		{
			ArgumentsAsList = argumentsAsList;
			ArgumentsAsString = argumentsAsString;
			CommandIdentifier = commandIdentifier;
			CommandText = commandText;
			ChatMessage = chatMessage;
		}

		public List<string> ArgumentsAsList { get; private set; }
		public string ArgumentsAsString { get; private set; }
		public char CommandIdentifier { get; private set; }
		public string CommandText { get; private set; }
		public ChatMessage ChatMessage { get; set; }
	}
}
