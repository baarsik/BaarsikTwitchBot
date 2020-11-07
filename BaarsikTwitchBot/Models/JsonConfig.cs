using System.Collections.Generic;

namespace BaarsikTwitchBot.Models
{
    public class JsonConfig
    {
        public ChannelCredentials Channel { get; set; }
        public OAuthCredentials OAuth { get; set; }
        public ICollection<string> UsersToIgnore { get; set; }
        public SongRequestManagerSettings SongRequestManager { get; set; }
        public string ConnectionString { get; set; }

        public class ChannelCredentials
        {
            public string Name { get; set; }
            public string OAuth { get; set; }
        }

        public class OAuthCredentials
        {
            public string ClientID { get; set; }
            public string ClientSecret { get; set; }
        }

        public class SongRequestManagerSettings
        {
            public string RewardTitleDefault { get; set; }
            public string RewardTitlePlus { get; set; }
            public int MaximumLengthInSeconds { get; set; }
            public bool DisplaySongName { get; set; }
        }
    }
}