using System;
using System.IO;
using BaarsikTwitchBot.Domain.Models;
using YoutubeExplode.Videos;

namespace BaarsikTwitchBot.Models
{
    public class SongRequest
    {
        ~SongRequest()
        {
            Stream?.Dispose();
        }

        public Guid RewardId { get; set; }

        public Video YoutubeVideo { get; set; }

        public SongRequestType RequestType { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public BotUser User { get; set; }

        public Stream Stream { get; set; }
    }
}