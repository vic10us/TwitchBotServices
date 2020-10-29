using ChatBotPrime.Core.Interfaces.Chat;
using System;
using Command = ChatBotPrime.Core.Events.EventArguments.ChatCommand;

namespace ChatBotPrime.Core.Services.CommandHandler
{
	public abstract class ChatCommand : IChatCommand
	{
		
		public abstract string CommandText { get; }
		public abstract string Response(IChatService service, Command chatMessage);
		public abstract TimeSpan Cooldown { get; set; }
		public DateTime LastRun { get; set; } = DateTime.UtcNow;
		public bool IsEnabled => (DateTime.UtcNow - LastRun) >= Cooldown;

		public bool IsMatch(string command)
		{

			return command.Equals(CommandText, StringComparison.InvariantCultureIgnoreCase);
		}

		protected void SetLastRun()
		{
			LastRun = DateTime.UtcNow;
		}

		protected bool CanRun()
		{
			if (Cooldown == TimeSpan.Zero)
				return true;

			return DateTime.UtcNow - LastRun <= Cooldown;
		}

		protected string GetTimeToRun()
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

	}
}
