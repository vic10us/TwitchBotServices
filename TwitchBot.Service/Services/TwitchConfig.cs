namespace TwitchBot.Service.Services
{
    public class TeamMember
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IgnoreShoutOut { get; set; }
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

    public class OBSConnection
    {
        public string Url { get; set; }
        public string Password { get; set; }
    }

    public class OBSConfig
    {
        public OBSConnection Connection { get; set; }
    }
    
    public class MotionFilter
    {
        public float acceleration { get; set; }
        public Backward[] backward { get; set; }
        public int dst_h { get; set; }
        public int dst_w { get; set; }
        public int dst_x { get; set; }
        public int dst_y { get; set; }
        public float duration { get; set; }
        public Forward[] forward { get; set; }
        public int motion_behavior { get; set; }
        public bool motion_end { get; set; }
        public float org_h { get; set; }
        public float org_w { get; set; }
        public float org_x { get; set; }
        public float org_y { get; set; }
        public int path_type { get; set; }
        public string scene_name { get; set; }
        public string source_id { get; set; }
        public int start_h { get; set; }
        public bool start_setting { get; set; }
        public int start_w { get; set; }
        public int start_x { get; set; }
        public int start_y { get; set; }
        public int variation_type { get; set; }
    }

    public class Backward
    {
        public bool alt { get; set; }
        public bool control { get; set; }
        public string key { get; set; }
        public bool shift { get; set; }
    }

    public class Forward
    {
        public bool alt { get; set; }
        public bool control { get; set; }
        public string key { get; set; }
        public bool shift { get; set; }
    }

}