using System.Collections.Generic;

namespace BaarsikTwitchBot.Models
{
    public class JsonConfig
    {
        public ChannelCredentials Channel { get; set; } = new ChannelCredentials();
        public OAuthCredentials OAuth { get; set; } = new OAuthCredentials();
        public ChatSettings Chat { get; set; } = new ChatSettings();
        public SongRequestManagerSettings SongRequestManager { get; set; } = new SongRequestManagerSettings();
        public string ConnectionString { get; set; } = "Server=localhost;Database=BaarsikTwitchBot;Trusted_Connection=True;Integrated Security=true;";

        public class ChannelCredentials
        {
            public string Name { get; set; } = Constants.User.ChannelName;
            public string OAuth { get; set; } = string.Empty;
        }

        public class OAuthCredentials
        {
            public string ClientID { get; set; } = string.Empty;
            public string ClientSecret { get; set; } = string.Empty;
            public string Scopes { get; set; } = "bits:read channel:read:hype_train channel:read:subscriptions user:read:broadcast channel:read:redemptions channel:moderate chat:edit channel:moderate whispers:edit channel_subscriptions chat:read whispers:read";
        }

        public class ChatSettings
        {
            public bool DisableUnsafeCommands { get; set; } = false;
            public ICollection<string> UsersToIgnore { get; set; } = new List<string>();
        }

        public class SongRequestManagerSettings
        {
            public string RewardTitleDefault { get; set; } = "Song Request";
            public string RewardTitlePlus { get; set; } = "Song Request+";
            public int MaximumLengthInSeconds { get; set; } = 360;
            public bool DisplaySongName { get; set; } = true;
        }
    }
}