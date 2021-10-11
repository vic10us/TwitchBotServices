using System;

namespace TwitchBot.Service.Extensions
{
    public struct HealthStatus
    {
        public DateTimeOffset StartedAt { get; set; }
        public TimeSpan UpTime => DateTimeOffset.UtcNow - StartedAt;
    }
}