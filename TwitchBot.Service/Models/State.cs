namespace TwitchBot.Service.Models
{
    public class State
    {
        public bool on { get; set; }
        public int bri { get; set; }
        public int transition { get; set; }
        public int ps { get; set; }
        public int pl { get; set; }
        public NightLight NightLight { get; set; }
        public Udpn udpn { get; set; }
        public Seg[] seg { get; set; }
    }
}