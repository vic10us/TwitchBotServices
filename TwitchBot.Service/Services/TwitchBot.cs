using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TwitchBot.Service.Features.MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Models;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Services
{
    public class TwitchBot
    {
        private readonly TwitchConfig _config;
        private readonly ILogger<TwitchBot> _logger;
        private readonly INotifierMediatorService _notifierMediatorService;
        private readonly TwitchClientServices _twitchClientServices;

        public TwitchBot(
            IOptions<TwitchConfig> config,
            ILogger<TwitchBot> logger,
            TwitchClientServices twitchClientServices,
            INotifierMediatorService notifierMediatorService, 
            TwitchPubSubService twitchPubSubService)
        {
            _config = config.Value;
            _logger = logger;
            _notifierMediatorService = notifierMediatorService;
            _twitchClientServices = twitchClientServices;

            twitchPubSubService.PubSubClient.OnRewardRedeemed += OnRewardRedeemed;

            _twitchClientServices.Client.OnChatCommandReceived += ClientOnChatCommandReceived;
            _twitchClientServices.Client.OnMessageReceived += ClientOnMessageReceived;
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            // var x = _twitchApiClient.Helix.Entitlements.GetCodeStatusAsync(new List<string> { $"{e.RewardId}" }, _config.Chat.PasswordGeneratorToken).Result;
            _twitchClientServices.Client.SendMessage(_config.Chat.Channel, $"Redeemed: {e.RewardId}");
        }

        private void ClientOnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            _notifierMediatorService.Notify(new ChatNotification(e.ChatMessage));
        }
        
        private void ClientOnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            _logger.LogInformation($"Received Chat Command: {JsonConvert.SerializeObject(e.Command, Formatting.Indented)}");
            _notifierMediatorService.Notify(new CallOutCommand(e.Command.ChatMessage.UserId, e.Command.ChatMessage.Username));

            switch (e.Command.CommandText.ToLowerInvariant())
            {
                case "drop":
                    _notifierMediatorService.Notify(new DropCommand(e.Command.ChatMessage));
                    break;
                case "yeet":
                    _notifierMediatorService.Notify(new YeetCommand(e.Command));
                    break;
                case "stats":
                    _notifierMediatorService.Notify(new StatsCommand());
                    break;
                case "grow":
                    _notifierMediatorService.Notify(new GrowCommand());
                    break;
                case "shrink":
                    _notifierMediatorService.Notify(new ShrinkCommand());
                    break;
                case "restore":
                    _notifierMediatorService.Notify(new RestoreCommand());
                    break;
                case "ledfx":
                    _notifierMediatorService.Notify(new LedFXCommand(e.Command.ArgumentsAsString));
                    break;
                default:
                    _notifierMediatorService.Notify(new UnknownChatCommand(e.Command.CommandText));
                    break;
            }
        }
    }
}
