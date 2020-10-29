using ChatBotPrime.Core.Events.EventArguments;
using System;
using System.Collections.Generic;

namespace ChatBotPrime.Core.Interfaces.Chat
{
	public interface IChatCommand
	{
		
		string CommandText { get; }
		TimeSpan Cooldown { get; set; }
		bool IsEnabled { get; }
		DateTime LastRun { get; set; }
		bool IsMatch(string command);
		string Response(IChatService service, ChatCommand chatMessage);
	}
}