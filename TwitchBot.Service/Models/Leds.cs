namespace TwitchBot.Service.Models
{
    public class Leds
    {
        public int count { get; set; }
        public bool rgbw { get; set; }
        public int[] pin { get; set; }
        public int pwr { get; set; }
        public int maxpwr { get; set; }
        public int maxseg { get; set; }
    }
}