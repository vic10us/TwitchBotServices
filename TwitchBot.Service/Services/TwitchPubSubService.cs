﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Models;
using TwitchLib.Api;
using TwitchLib.PubSub;

namespace TwitchBot.Service.Services
{
    public class TwitchPubSubService
    {
        private readonly TwitchConfig _config;
        private readonly ILogger<TwitchPubSubService> _logger;
        public TwitchPubSub PubSubClient { get; }

        public TwitchPubSubService(
            TwitchAPI twitchApiClient, 
            IOptions<TwitchConfig> config,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TwitchPubSubService>();
            _config = config.Value;
            PubSubClient = new TwitchPubSub();
            var user = twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { "thisistherealvoice" }).Result.Users.First();
            PubSubClient.OnPubSubServiceConnected += OnPubSubServiceConnected;
            PubSubClient.ListenToRewards(user.Id);
            // PubSubClient.ListenToChannelPoints(user.Id);
            PubSubClient.OnPubSubServiceClosed += (sender, args) =>
            {
                _logger.LogWarning("The service has closed it's connection... :(", args);
            };
            PubSubClient.OnPubSubServiceError += (sender, args) =>
            {
                _logger.LogError("The service encountered a critical error... :(", args);
            };
            PubSubClient.OnChannelSubscription += (sender, args) =>
            {
                
            };

            PubSubClient.Connect();
        }

        private void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            // SendTopics accepts an oauth optionally, which is necessary for some topics
            PubSubClient.SendTopics(_config.Chat.PasswordGeneratorToken);
        }
    }
}