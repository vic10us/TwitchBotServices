using TwitchBot.Service.Services;

namespace TwitchBot.Service.Models
{
    public class TwitchConfig
    {
        public OAuthConfig Auth { get; set; }
        public TwitchChatConfig Chat { get; set; }
        public string[] IgnoredUsers { get; set; }
        public string TeamName { get; set; }
        public bool TeamShoutoutEnabled { get; set; }
        public TeamMember[] TeamMembers { get; set; }
    }
}