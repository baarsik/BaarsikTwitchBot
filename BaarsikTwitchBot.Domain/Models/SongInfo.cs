using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Domain.Models.Abstract;

namespace BaarsikTwitchBot.Domain.Models
{
    public class SongInfo : BaseEntity
    {
        public string VideoId { get; set; }
        public SongLimitationType Limitation { get; set; } = SongLimitationType.Default;
    }
}