using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotPrime.Core.Configuration
{
	public class DiscordSettings
	{
		public bool Enabled { get; set; }
		public string Username { get; set; }
		public string Token { get; set; }
		public string Server { get; set; }
		public string  Channel { get; set; }
		public char CommandIdentifier { get; set; }
	}
}
