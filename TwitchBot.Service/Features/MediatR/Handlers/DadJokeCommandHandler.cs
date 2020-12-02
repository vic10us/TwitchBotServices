using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Features.MediatR.Commands;
using TwitchBot.Service.Hubs;
using TwitchBot.Service.Models;
using TwitchBot.Service.Services;
using TwitchLib.Api;
using TwitchLib.Client;

namespace TwitchBot.Service.Features.MediatR.Handlers
{
    public class DadJokeCommandHandler : INotificationHandler<DadJokeCommand>
    {
        private readonly IMapper _mapper;
        private readonly INotifierMediatorService _notifierMediatorService;
        private readonly TwitchAPI _twitchApiClient;
        private readonly DadJokeService _service;
        private readonly TwitchConfig _config;
        private readonly TwitchClient _twitchClient;

        public DadJokeCommandHandler(TwitchClientServices twitchClient, IMapper mapper, INotifierMediatorService notifierMediatorService, TwitchAPI twitchApiClient, IHubContext<TwitchHub> twitchHub, DadJokeService service, IOptions<TwitchConfig> config)
        {
            _twitchClient = twitchClient.Client;
            _mapper = mapper;
            _notifierMediatorService = notifierMediatorService;
            _twitchApiClient = twitchApiClient;
            _service = service;
            _config = config.Value;
        }

        public async Task Handle(DadJokeCommand notification, CancellationToken cancellationToken)
        {
            try
            {
                var dadJoke = await _service.GetDadJoke();
                if (dadJoke == null) return;
                var uo = _twitchApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { notification.ChatCommand.ChatMessage.DisplayName }).Result.Users.FirstOrDefault();
                if (uo == null) return;

                var message = $"@{uo.DisplayName} Here's your dad joke: \"{dadJoke.Joke}\"";
                _twitchClient.SendMessage(_config.Chat.Channel, message);
                return;
                
                var emotes = _mapper.Map<IEnumerable<EmoteDto>>(notification.ChatCommand.ChatMessage.EmoteSet.Emotes);

                var userTypes = UserTypes.None;
                if (notification.ChatCommand.ChatMessage.IsBroadcaster) userTypes |= UserTypes.Broadcaster;
                if (notification.ChatCommand.ChatMessage.IsModerator) userTypes |= UserTypes.Moderator;
                if (notification.ChatCommand.ChatMessage.IsSubscriber) userTypes |= UserTypes.Subscriber;
                if (notification.ChatCommand.ChatMessage.IsVip) userTypes |= UserTypes.Vip;
                if (_config.TeamMembers.Any(tm =>
                    tm.Id.Equals(notification.ChatCommand.ChatMessage.UserId, StringComparison.InvariantCultureIgnoreCase)))
                    userTypes |= UserTypes.TeamMember;

                var data = new ChatMessageData
                {
                    MessageId = notification.ChatCommand.ChatMessage.Id,
                    UserId = notification.ChatCommand.ChatMessage.UserId,
                    UserName = notification.ChatCommand.ChatMessage.Username,
                    DisplayName = notification.ChatCommand.ChatMessage.DisplayName,
                    Message = dadJoke.Joke,
                    TeamName = _config.TeamName,
                    TeamShoutoutEnabled = _config.TeamShoutoutEnabled,
                    EmoteDetails = emotes.ToArray(),
                    UserTypes = userTypes
                };

                _notifierMediatorService.Notify(new BasicChatNotification(data, uo));
                // await _twitchHub.Clients.All.SendAsync("ReceiveMessage", notification.ChatCommand.ChatMessage.DisplayName, "dadjoke", uo, cancellationToken: cancellationToken);
            }
            catch
            {
                // do nothing
            }
        }
    }
}