namespace TwitchBot.Service.Models
{
    public class WLEDRootObject
    {
        public State state { get; set; }
        public Info info { get; set; }
        public string[] effects { get; set; }
        public string[] palettes { get; set; }
    }
}
