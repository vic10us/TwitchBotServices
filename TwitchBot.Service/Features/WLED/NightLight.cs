﻿namespace TwitchBot.Service.Features.WLED
{
    public class NightLight
    {
        public bool on { get; set; }
        public int dur { get; set; }
        public bool fade { get; set; }
        public int tbri { get; set; }
    }
}