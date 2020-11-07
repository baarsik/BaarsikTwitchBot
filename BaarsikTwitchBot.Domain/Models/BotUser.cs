using System.ComponentModel.DataAnnotations;
using BaarsikTwitchBot.Domain.Models.Abstract;

namespace BaarsikTwitchBot.Domain.Models
{
    public class BotUser : BaseEntity
    {
        public string UserId { get; set; }
        public string Login { get; set; }
        public string DisplayName { get; set; }
        public bool IsFollower { get; set; }
        public bool IsBanned { get; set; } = false;
        public bool IsBannedSongPlayer { get; set; } = false;
        public BotUserStatistics Statistics { get; set; } = new BotUserStatistics();
    }
}