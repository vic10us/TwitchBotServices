namespace TwitchBot.Service.Features.WLED
{
    public class WLEDRootObject
    {
        public State state { get; set; }
        public Info info { get; set; }
        public string[] effects { get; set; }
        public string[] palettes { get; set; }
    }
}
