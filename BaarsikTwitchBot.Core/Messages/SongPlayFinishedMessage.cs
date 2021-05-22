using System;
using System.Collections.Generic;
using BaarsikTwitchBot.Core.Models;

namespace BaarsikTwitchBot.Core.Messages
{
    public class SongPlayFinishedMessage : BaseMessage
    {
        public IList<SongRequest> Queue { get; set; }
        
        public SongRequest CurrentRequest { get; set; }
        
        public TimeSpan? CurrentRequestTimeSpan { get; set; }
        
        public bool IsPlaying { get; set; }
    }
}