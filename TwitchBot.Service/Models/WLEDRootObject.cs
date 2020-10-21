using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
