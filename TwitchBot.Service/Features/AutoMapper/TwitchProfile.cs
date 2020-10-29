using AutoMapper;
using TwitchBot.Service.Models;
using TwitchLib.Client.Models;

namespace TwitchBot.Service.Features.AutoMapper
{
    public class TwitchProfile : Profile
    {
        public TwitchProfile()
        {
            CreateMap<Emote, EmoteDto>();
            CreateMap<ChatMessage, ChatBotPrime.Core.Events.EventArguments.ChatMessage>();
        }
    }
}
