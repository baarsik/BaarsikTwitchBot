using System.Collections.Generic;
using System.Reflection;

namespace BaarsikTwitchBot.Models
{
    public class JsonConfig
    {
        [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
        public ChannelCredentials BotUser { get; set; } = new()
        {
            Scopes = Constants.Twitch.Scopes.BotUser
        };

        [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
        public ChannelCredentials Channel { get; set; } = new()
        {
            Name = Constants.User.ChannelName,
            Scopes = Constants.Twitch.Scopes.Channel
        };

        [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
        public OAuthCredentials OAuth { get; set; } = new();

        [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
        public ChatSettings Chat { get; set; } = new();

        [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
        public SongRequestManagerSettings SongRequestManager { get; set; } = new();

        [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
        public TwitchEmoteSettings TwitchEmotes { get; set; } = new();

        [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
        public int WebServerLocalPort { get; set; } = 14828;

        public class ChannelCredentials
        {
            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string Name { get; set; } = string.Empty;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string OAuth { get; set; } = string.Empty;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string Scopes { get; set; } = string.Empty;
        }

        public class OAuthCredentials
        {
            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string ClientID { get; set; } = string.Empty;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string ClientSecret { get; set; } = string.Empty;
        }

        public class ChatSettings
        {
            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public bool DisableUnsafeCommands { get; set; } = false;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public ICollection<string> UsersToIgnore { get; set; } = new List<string>();
        }

        public class SongRequestManagerSettings
        {
            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public bool Enabled { get; set; } = true;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string RewardTitleDefault { get; set; } = "Song Request";

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string RewardTitlePlus { get; set; } = "Song Request+";

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)] 
            public int MaximumLengthInSeconds { get; set; } = 360;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public int MaximumLengthInSecondsPlus { get; set; } = 450;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)] 
            public bool DisplaySongName { get; set; } = true;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)] 
            public bool AllowDuplicatesDefault { get; set; } = false;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)] 
            public bool AllowDuplicatesPlus { get; set; } = true;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public uint SoundVolume { get; set; } = 25;

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public int YoutubeMinimumViews { get; set; } = 1000;
        }

        public class TwitchEmoteSettings
        {
            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string LUL { get; set; } = "LUL";

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string Love { get; set; } = "baarsiLove";

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string Gasm { get; set; } = "baarsiGasm";

            [Obfuscation(Feature = Constants.Obfuscation.Renaming, Exclude = true)]
            public string PogChamp { get; set; } = "PogChamp";
        }
    }
}