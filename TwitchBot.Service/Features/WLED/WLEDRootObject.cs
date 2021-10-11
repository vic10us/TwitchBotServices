namespace TwitchBot.Service.Features.WLED
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WLEDRootObject
    {
        public State state { get; set; }
        public Info info { get; set; }
        public string[] effects { get; set; }
        public string[] palettes { get; set; }
    }
}
