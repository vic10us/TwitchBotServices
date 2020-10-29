using ChatBotPrime.Core.Events.EventArguments;

namespace ChatBotPrime.Core.Interfaces.Chat
{
	public interface IChatMessage
	{
		bool IsMatch(string messageText);
		string MessageText { get; }

		string Response(IChatService service, ChatMessage chatMessage);
	}
}