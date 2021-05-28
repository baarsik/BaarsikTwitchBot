namespace BaarsikTwitchBot.Models
{
    public enum BotInitializationStatus
    {
        NotInitialized,
        ChannelCredentialsValidated,
        BotUserCredentialsValidated,
        Initialized
    }
}