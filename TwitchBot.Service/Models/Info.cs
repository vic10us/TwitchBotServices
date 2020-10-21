namespace TwitchBot.Service.Models
{
    public class Info
    {
        public string ver { get; set; }
        public int vid { get; set; }
        public Leds leds { get; set; }
        public string name { get; set; }
        public int udpport { get; set; }
        public bool live { get; set; }
        public int fxcount { get; set; }
        public int palcount { get; set; }
        public string arch { get; set; }
        public string core { get; set; }
        public int freeheap { get; set; }
        public int uptime { get; set; }
        public int opt { get; set; }
        public string brand { get; set; }
        public string product { get; set; }
        public string btype { get; set; }
        public string mac { get; set; }
    }
}