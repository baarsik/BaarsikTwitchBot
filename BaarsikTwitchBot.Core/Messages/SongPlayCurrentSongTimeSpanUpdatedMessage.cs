using System;

namespace BaarsikTwitchBot.Core.Messages
{
    public class SongPlayCurrentSongTimeSpanUpdatedMessage : BaseMessage
    {
        public TimeSpan? CurrentRequestTimeSpan { get; set; }
    }
}