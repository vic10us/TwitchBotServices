using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotPrime.Core.Configuration
{
	public class TwitchSettings
	{
		public bool Enabled { get; set; }
		public string BotName { get; set; }
		public string ClientId { get; set; }
		public string Username { get; set; }
		public string Token { get; set; }
		public string  Channel { get; set; }
		public char CommandIdentifier { get; set; }
	}
}
