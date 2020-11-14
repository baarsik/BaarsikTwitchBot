using System.Collections.Generic;
using Newtonsoft.Json;

namespace BaarsikTwitchBot.Models
{
    public class JsonConfig
    {
        [JsonProperty("Channel")]
        public ChannelCredentials Channel { get; set; } = new ChannelCredentials();

        [JsonProperty("OAuth")]
        public OAuthCredentials OAuth { get; set; } = new OAuthCredentials();

        [JsonProperty("Chat")]
        public ChatSettings Chat { get; set; } = new ChatSettings();

        [JsonProperty("SongRequestManager")]
        public SongRequestManagerSettings SongRequestManager { get; set; } = new SongRequestManagerSettings();

        [JsonProperty("TwitchEmotes")]
        public TwitchEmoteSettings TwitchEmotes { get; set; } = new TwitchEmoteSettings();

        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; set; } = "Server=localhost;Database=BaarsikTwitchBot;Trusted_Connection=True;Integrated Security=true;";

        public class ChannelCredentials
        {
            [JsonProperty("Name")]
            public string Name { get; set; } = Constants.User.ChannelName;

            [JsonProperty("OAuth")]
            public string OAuth { get; set; } = string.Empty;
        }

        public class OAuthCredentials
        {
            [JsonProperty("ClientID")]
            public string ClientID { get; set; } = string.Empty;

            [JsonProperty("ClientSecret")]
            public string ClientSecret { get; set; } = string.Empty;

            [JsonProperty("Scopes")]
            public string Scopes { get; set; } = "bits:read channel:read:hype_train channel:read:subscriptions user:read:broadcast channel:read:redemptions channel:moderate chat:edit channel:moderate whispers:edit channel_subscriptions chat:read whispers:read";
        }

        public class ChatSettings
        {
            [JsonProperty("DisableUnsafeCommands")]
            public bool DisableUnsafeCommands { get; set; } = false;

            [JsonProperty("UsersToIgnore")]
            public ICollection<string> UsersToIgnore { get; set; } = new List<string>();
        }

        public class SongRequestManagerSettings
        {
            [JsonProperty("Enabled")]
            public bool Enabled { get; set; } = true;

            [JsonProperty("RewardTitleDefault")]
            public string RewardTitleDefault { get; set; } = "Song Request";

            [JsonProperty("RewardTitlePlus")]
            public string RewardTitlePlus { get; set; } = "Song Request+";

            [JsonProperty("MaximumLengthInSeconds")] 
            public int MaximumLengthInSeconds { get; set; } = 360;

            [JsonProperty("DisplaySongName")] 
            public bool DisplaySongName { get; set; } = true;

            [JsonProperty("AllowDuplicatesDefault")] 
            public bool AllowDuplicatesDefault { get; set; } = false;

            [JsonProperty("AllowDuplicatesPlus")] 
            public bool AllowDuplicatesPlus { get; set; } = true;
        }

        public class TwitchEmoteSettings
        {
            [JsonProperty("LUL")]
            public string LUL { get; set; } = "LUL";

            [JsonProperty("Love")]
            public string Love { get; set; } = "baarsiLove";

            [JsonProperty("Gasm")]
            public string Gasm { get; set; } = "baarsiGasm";

            [JsonProperty("PogChamp")]
            public string PogChamp { get; set; } = "PogChamp";
        }
    }
}