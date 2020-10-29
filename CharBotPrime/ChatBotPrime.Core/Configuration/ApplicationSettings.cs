using System;

namespace ChatBotPrime.Core.Configuration
{
	public class ApplicationSettings
	{
		public TwitchSettings TwitchSettings { get; set; }
		public DiscordSettings DiscordSettings { get; set; }

		public SignalRSettings SignalRSettings { get; set; }
	}

}
