using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TwitchBot.Service.Models
{
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
