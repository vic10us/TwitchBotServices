using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TwitchBot.Service.Models
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserTypes : uint
    {
        None = 0,
        Broadcaster = 1 << 4,
        Moderator = 1 << 8,
        Vip = 1 << 12,
        Subscriber = 1 << 16,
        TeamMember = 1 << 20,
        All = Broadcaster | Moderator | Vip | Subscriber | TeamMember,
    }
}