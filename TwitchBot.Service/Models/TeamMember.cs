namespace TwitchBot.Service.Models
{
    public class TeamMember
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IgnoreShoutOut { get; set; }
    }
}