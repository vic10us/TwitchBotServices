

using ChatBotPrime.Core.Configuration;
using ChatBotPrime.Core.Events.EventArguments;
using ChatBotPrime.Core.Interfaces.Chat;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChatBotPrime.Infra.Chat.Discord
{
	public class DiscordChatService : IChatService
	{
		private DiscordSocketClient _client;
		private DiscordSettings _settings;
		private ILogger<DiscordChatService> _logger;

		public DiscordChatService(IOptions<ApplicationSettings> applicationSettingsAccessor,ILogger<DiscordChatService> logger)
		{
			_settings = applicationSettingsAccessor.Value.DiscordSettings;
			_logger = logger;
			_client = new DiscordSocketClient();
		}

		public bool _connected => throw new NotImplementedException();

		public event EventHandler<ChatCommandReceivedEventArgs> OnCommandReceived;
		public event EventHandler<ChatMessageReceivedEventArgs> OnMessageReceived;

		public void Connect()
		{
			ConfigureHandlers();
			_client.LoginAsync(TokenType.Bot, _settings.Token);
			_client.StartAsync();
		}

		private void ConfigureHandlers()
		{
			_client.MessageReceived += MessageReceived;
		}

		private Task MessageReceived(SocketMessage message)
		{
			// The bot should never respond to itself.
			if (message.Author.Id == _client.CurrentUser.Id)
				return Task.CompletedTask;

			_logger.LogInformation($"Received Message!");

			if (message.Content == "!ping")
				    message.Channel.SendMessageAsync("pong!");
			
			return Task.CompletedTask;
		}

		public void Disconnect()
		{
			throw new NotImplementedException();
		}

		public void JoinChannel(string channel)
		{
			throw new NotImplementedException();
		}

		public void SendMessage(string message)
		{
			throw new NotImplementedException();
		}

		public void SendMessage(string channel, string message)
		{
			throw new NotImplementedException();
		}
	}
}
