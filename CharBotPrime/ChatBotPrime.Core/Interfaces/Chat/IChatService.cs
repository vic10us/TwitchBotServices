using ChatBotPrime.Core.Events.EventArguments;
using System;

namespace ChatBotPrime.Core.Interfaces.Chat
{
	public interface IChatService
	{
		bool _connected { get; }

		event EventHandler<ChatCommandReceivedEventArgs> OnCommandReceived;
		event EventHandler<ChatMessageReceivedEventArgs> OnMessageReceived;

		void Connect();
		void Disconnect();
		void JoinChannel(string channel);
		//void CommandReceived(ChatCommandReceivedEventArgs e);
		//void MessageReceived(ChatMessageReceivedEventArgs e);
		void SendMessage(string message);
		void SendMessage(string channel, string message);
	}
}