using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotPrime.Core.Events.EventArguments
{
	public class ChatCommandReceivedEventArgs	: EventArgs
	{
		public ChatCommandReceivedEventArgs(ChatCommand chatCommand)
		{
			ChatCommand = chatCommand;
		}

		public ChatCommand ChatCommand { get; private set; }
	}
}
