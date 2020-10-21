namespace TwitchBot.Service.Models
{
    public class Seg
    {
        public int id { get; set; }
        public int start { get; set; }
        public int stop { get; set; }
        public int len { get; set; }
        public int[][] col { get; set; }
        public int fx { get; set; }
        public int sx { get; set; }
        public int ix { get; set; }
        public int pal { get; set; }
        public bool sel { get; set; }
        public bool rev { get; set; }
        public int cln { get; set; }
    }
}