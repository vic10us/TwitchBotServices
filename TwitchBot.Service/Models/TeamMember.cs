namespace TwitchBot.Service.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TeamMember
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IgnoreShoutOut { get; set; }
    }
}