using BaarsikTwitchBot.Domain.Models.Abstract;

namespace BaarsikTwitchBot.Domain.Models
{
    public class BotUserStatistics : BaseEntity
    {
        public uint SpitsReceived { get; set; } = 0;
        public uint LicksReceived { get; set; } = 0;
    }
}