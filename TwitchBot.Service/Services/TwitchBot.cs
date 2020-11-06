using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TwitchBot.Service.Features.Caching;
using TwitchBot.Service.Features.MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Models;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Services
{
    public class TwitchBot
    {
        private readonly ILogger<TwitchBot> _logger;
        private readonly INotifierMediatorService _notifierMediatorService;

        public TwitchBot(
            ILogger<TwitchBot> logger,
            TwitchClientServices twitchClientServices,
            INotifierMediatorService notifierMediatorService, 
            TwitchPubSubService twitchPubSubService)
        {
            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
            //IDatabase db = redis.GetDatabase();
            // var manager = new RedisManagerPool("localhost:6379");
            _logger = logger;
            _notifierMediatorService = notifierMediatorService;
            var twitchClientServices1 = twitchClientServices;
            twitchPubSubService.PubSubClient.OnRewardRedeemed += OnRewardRedeemed;

            twitchClientServices1.Client.OnChatCommandReceived += ClientOnChatCommandReceived;
            twitchClientServices1.Client.OnMessageReceived += ClientOnMessageReceived;
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            if (!e.Status.Equals("UNFULFILLED")) return;
            _notifierMediatorService.NotifyPattern(e.RewardTitle.ToLowerInvariant());
        }

        private void ClientOnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            _notifierMediatorService.Notify(new ChatNotification(e.ChatMessage));
        }
        
        private void ClientOnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            _logger.LogInformation($"Received Chat Command: {JsonConvert.SerializeObject(e.Command, Formatting.Indented)}");
            _notifierMediatorService.Notify(new CallOutCommand(e.Command));
            _notifierMediatorService.NotifyPattern(e.Command.CommandText.ToLowerInvariant(), e.Command);
        }
    }
}
