namespace TwitchBot.Service.Services
{
    public class TeamMember
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

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