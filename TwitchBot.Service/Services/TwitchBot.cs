using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchBot.Service.Features.MediatR;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Service.Services
{
    public class TwitchBot
    {
        private readonly ILogger<TwitchBot> _logger;
        private readonly INotifierMediatorService _notifierMediatorService;
        private readonly TwitchClientServices _twitchClientServices;

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
            _twitchClientServices = twitchClientServices;
            _twitchClientServices.Init();

            twitchPubSubService.PubSubClient.OnRewardRedeemed += OnRewardRedeemed;

            _twitchClientServices.Client.OnJoinedChannel += ClientOnJoinedChannel;
            _twitchClientServices.Client.OnChatCommandReceived += ClientOnChatCommandReceived;
            _twitchClientServices.Client.OnMessageReceived += ClientOnMessageReceived;
            
            _twitchClientServices.Connect();
        }

        private void ClientOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            _twitchClientServices.Client.SendMessage(e.Channel, "Hello peeps! Yeet all the thingz!");
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            if (!e.Status.Equals("UNFULFILLED")) return;
            _notifierMediatorService.NotifyPattern(e.RewardTitle.ToLowerInvariant(), e);
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
