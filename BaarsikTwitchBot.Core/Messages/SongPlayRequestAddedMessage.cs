using System.Collections.Generic;
using BaarsikTwitchBot.Core.Models;

namespace BaarsikTwitchBot.Core.Messages
{
    public class SongPlayRequestAddedMessage : BaseMessage
    {
        public IList<SongRequest> Queue { get; set; }
    }
}