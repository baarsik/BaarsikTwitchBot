namespace BaarsikTwitchBot
{
    public static class Constants
    {
        public static class RewardStatus
        {
            public const string Fulfilled = "FULFILLED";
            public const string Unfulfilled = "UNFULFILLED";
        }

        public static class User
        {
            public const string ChannelName = "baarsik";
        }

        public static class Twitch
        {
            public const int FollowerRequestLimit = 100;

            public static class Scopes
            {
                public const string BotUser = "chat:read chat:edit whispers:edit bits:read user:read:broadcast channel:moderate";
                public const string Channel = "chat:read chat:edit whispers:edit bits:read channel:read:hype_train channel:read:subscriptions user:read:broadcast channel:read:redemptions channel:manage:redemptions channel:moderate";
            }
        }

        public static class Obfuscation
        {
            public const string Mutation = "mutation";
            public const string Virtualization = "virtualization";
            public const string Ultra = "ultra";
            public const string Virtualizationlockbykey = "virtualizationlockbykey";
            public const string Ultralockbykey = "ultralockbykey";
            public const string Renaming = "renaming";
        }
    }
}