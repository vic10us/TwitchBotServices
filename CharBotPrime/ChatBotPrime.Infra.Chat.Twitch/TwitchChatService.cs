using ChatBotPrime.Core.Configuration;
using ChatBotPrime.Core.Interfaces.Chat;
using ChatBotPrime.Core.Events.EventArguments;
using Microsoft.Extensions.Options;
using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Microsoft.Extensions.Logging;
using ChatBotPrime.Core.Interfaces.Stream;
using TwitchLib.Api;
using System.Linq;
using System.Collections.Generic;
using Command = ChatBotPrime.Core.Events.EventArguments.ChatCommand;

namespace ChatBotPrime.Infra.Chat.Twitch
{
	public class TwitchChatService : IStreamService 
	{
		private TwitchClient _client;
		private TwitchAPI _api = new TwitchAPI();
		private readonly TwitchSettings _settings;
		private JoinedChannel _channel;
		private ILogger<TwitchChatService> _logger;
		public bool _connected => _client.IsConnected;

		public event EventHandler<ChatMessageReceivedEventArgs> OnMessageReceived;
		public event EventHandler<ChatCommandReceivedEventArgs> OnCommandReceived;

		public TwitchChatService(IOptions<ApplicationSettings> applicationSettingsAccessor, ILogger<TwitchChatService> logger)
		{
			_settings = applicationSettingsAccessor.Value.TwitchSettings;
			_logger = logger;

			var creds = CreateCredentials();
			CreateClient();
			if (_client != null)
			{
				Initialize(creds);
				ConfigureHandlers();
				_client.AddChatCommandIdentifier(_settings.CommandIdentifier);
			}

		}

		public void Connect()
		{
			if (_client != null)
			{
				_client.Connect();
			}
		}

		public void Disconnect()
		{
			_client.Disconnect();
		}

		public void JoinChannel(string channel)
		{
			_client.JoinChannel(channel);
			_logger.LogInformation($"Joined channel : {channel}");
		}

		private void CommandReceived(ChatCommandReceivedEventArgs e)
		{
			var handelr = OnCommandReceived;
			handelr?.Invoke(this, e);
		}

		private void MessageReceived(ChatMessageReceivedEventArgs e)
		{
			var handler = OnMessageReceived;
			handler?.Invoke(this, e);
		}

		public void SendMessage(string message)
		{
			SendMessage(_channel.Channel, message);
		}

		public void SendMessage(string channel, string message)
		{
			_client.SendMessage(channel, message);
		}


		private ConnectionCredentials CreateCredentials()
		{
			return new ConnectionCredentials(_settings.Username, _settings.Token);
		}

		private void CreateClient()
		{
			var clientOptions = new ClientOptions
			{
				MessagesAllowedInPeriod = 750,
				ThrottlingPeriod = TimeSpan.FromSeconds(30)
			};
			WebSocketClient customClient = new WebSocketClient(clientOptions);
			_client = new TwitchClient(customClient);

		}

		private void Initialize(ConnectionCredentials creds)
		{
			_client.Initialize(creds);
			_api.Settings.ClientId = _settings.ClientId;
			_api.Settings.AccessToken = _settings.Token;
		}

		private void ConfigureHandlers()
		{
			_client.OnMessageReceived += MessageReceived;
			_client.OnConnected += OnConnected;
			_client.OnChatCommandReceived += CommandReceived;
		}

		private void OnConnected(object sender, OnConnectedArgs args)
		{
			_logger.LogInformation($"Connection To Twitch Started.");
			JoinChannel(_settings.Channel);
			_channel = _client.GetJoinedChannel(_settings.Channel);
			SendMessage($"{_settings.BotName} has arrived");
		}


		private void CommandReceived(object sender, OnChatCommandReceivedArgs args)
		{
			_logger.LogInformation($"Command Received from Chat : {args.Command.CommandText}  arguments : {args.Command.ArgumentsAsString}");
			var eventArgs = new ChatCommandReceivedEventArgs( new Command(
					args.Command.ArgumentsAsList,
					args.Command.ArgumentsAsString,
					args.Command.CommandIdentifier,
					args.Command.CommandText,
					new Core.Events.EventArguments.ChatMessage(
						args.Command.ChatMessage.Message,
						args.Command.ChatMessage.IsVip,
						args.Command.ChatMessage.IsSubscriber,
						args.Command.ChatMessage.IsModerator,
						args.Command.ChatMessage.IsMe,
						args.Command.ChatMessage.IsBroadcaster,
						args.Command.ChatMessage.SubscribedMonthCount,
						args.Command.ChatMessage.Id,
						args.Command.ChatMessage.Channel,
						args.Command.ChatMessage.Bits,
						args.Command.ChatMessage.IsHighlighted,
						args.Command.ChatMessage.UserId,
						args.Command.ChatMessage.Username
				)));

			CommandReceived(eventArgs);
		}

		private void MessageReceived(object sender, OnMessageReceivedArgs args)
		{
			string msg = args.ChatMessage.Message;

			if (!msg.StartsWith(_settings.CommandIdentifier.ToString()))
			{
				_logger.LogInformation($"Message Received from Chat user : {args.ChatMessage.Username} message: {args.ChatMessage.Message}");

				var eventArgs = new ChatMessageReceivedEventArgs( new Core.Events.EventArguments.ChatMessage(
						args.ChatMessage.Message,
						args.ChatMessage.IsVip,
						args.ChatMessage.IsSubscriber,
						args.ChatMessage.IsModerator,
						args.ChatMessage.IsMe,
						args.ChatMessage.IsBroadcaster,
						args.ChatMessage.SubscribedMonthCount,
						args.ChatMessage.Id,
						args.ChatMessage.Channel,
						args.ChatMessage.Bits,
						args.ChatMessage.IsHighlighted,
						args.ChatMessage.UserId,
						args.ChatMessage.Username
						));

				MessageReceived(eventArgs);

			}
		}

		public string UpTime()
		{
			var channel = _api.V5.Users.GetUserByNameAsync(_settings.Channel).Result.Matches.FirstOrDefault();
			if (channel == null) throw new KeyNotFoundException("Channel not found");

			var online = _api.V5.Streams.BroadcasterOnlineAsync(channel.Id).Result;
			if (!online) return "offline";

			return ((TimeSpan)_api.V5.Streams.GetUptimeAsync(channel.Id).Result).ToString(@"dd\.hh\:mm\:ss");
		}
	}
}