using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TwitchBot.Service.Models
{
    public class EmoteDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string ImageUrl { get; set; }

        public EmoteDto() { }

        public EmoteDto(string emoteId, string name, int emoteStartIndex, int emoteEndIndex)
        {
            Id = emoteId;
            Name = name;
            StartIndex = emoteStartIndex;
            EndIndex = emoteEndIndex;
            ImageUrl = "https://static-cdn.jtvnw.net/emoticons/v1/" + emoteId + "/1.0";
        }
    }

    public class ChatMessageData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
        public string LogoUrl { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UserTypes UserTypes { get; set; } = UserTypes.None;
        public string TeamName { get; set; }
        public bool TeamShoutoutEnabled { get; set; }

        public EmoteDto[] EmoteDetails { get; set; }

        public Dictionary<string, string[]> Emotes
        {
            get
            {
                if (EmoteDetails == null) return new Dictionary<string, string[]>();
                var items = EmoteDetails.GroupBy(e => e.Id).ToDictionary(e => e.Key, e => e.Select(em => $"{em.StartIndex}-{em.EndIndex}").ToArray());
                return items;
            }
        }
    }
}
