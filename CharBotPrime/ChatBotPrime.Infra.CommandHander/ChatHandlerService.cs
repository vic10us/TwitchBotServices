using ChatBotPrime.Core.Data;
using ChatBotPrime.Core.Data.Specifications;
using ChatBotPrime.Core.Events.EventArguments;
using ChatBotPrime.Core.Interfaces.Chat;
using ChatBotPrime.Core.Interfaces.Stream;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ChatBotPrime.Infra.ChatHander
{
	public class ChatHandlerService
	{
		private IEnumerable<IChatService> _chatServices;
		private List<IChatCommand> _commands;
		private List<IChatMessage> _messages;
		private ILogger<ChatHandlerService> _logger;


		public ChatHandlerService(IRepository repository, IEnumerable<IChatService> chatServices, IEnumerable<IChatCommand> commands, IEnumerable<IChatMessage> messages, ILogger<ChatHandlerService> logger)
		{
			_logger = logger;
			_chatServices = chatServices;

			_commands = commands.ToList();
			_messages = messages.ToList();
			

			AddEventHandlersToChatServices();

			_commands.AddRange(repository.ListAsync(BasicCommandPolicy.All()).Result.AsEnumerable());
			_messages.AddRange(repository.ListAsync(BasicMessagePolicy.All()).Result.AsEnumerable());

			_logger.LogInformation($"Number of chat Services added to chat handler: {_chatServices.Count()}");
			_logger.LogInformation($"Number of chat commands added to chat handler: {_commands.Count}");
			_logger.LogInformation($"Number of chat messages added to chat handler: {_messages.Count}");
		}

		public void ConfigureChatSystem(IEnumerable<IChatCommand> commandsToAdd,IEnumerable<IChatMessage> messagesToAdd)
		{
			_commands.AddRange(commandsToAdd);
			_messages.AddRange(messagesToAdd);

			
		}

		private void AddEventHandlersToChatServices()
		{
			foreach (IChatService svc in _chatServices)
			{
				svc.OnCommandReceived += CommandHander;
				svc.OnMessageReceived += MessageHandler;
			}
		}

		private void CommandHander(object sender, ChatCommandReceivedEventArgs e)
		{
			if (sender is IChatService service)
			{
				var command = GetCommand(e.ChatCommand.CommandText);

				if (command is IStreamCommand)
				{
					if (!(service is IStreamService))
					{	
						service.SendMessage("Command is for use in stream based Service and cannot be run from a chat only Service");
					}
				}

				service.SendMessage(command.Response(service,e.ChatCommand));
			}
		}

		private IChatCommand GetCommand(string commandText)
		{
			return _commands.First(c => c.IsMatch(commandText));
		}


		private void MessageHandler(object sender, ChatMessageReceivedEventArgs e)
		{
			if (sender is IChatService service)
			{
				var message = GetMessage(e.ChatMessage.Message);

				if (!(message is null))
				{
					service.SendMessage(message.Response(service,e.ChatMessage));
				}
			}
		}

		private IChatMessage GetMessage(string messageText)
		{
			return _messages.FirstOrDefault(m => m.IsMatch(messageText));
		}
	}
}
