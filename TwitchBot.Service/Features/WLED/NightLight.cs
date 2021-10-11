namespace TwitchBot.Service.Features.WLED
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NightLight
    {
        public bool on { get; set; }
        public int dur { get; set; }
        public bool fade { get; set; }
        public int tbri { get; set; }
    }
}