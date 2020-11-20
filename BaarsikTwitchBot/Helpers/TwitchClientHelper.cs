using TwitchLib.Client;

namespace BaarsikTwitchBot.Helpers
{
    public class TwitchClientHelper
    {
        private readonly TwitchClient _client;

        public TwitchClientHelper(TwitchClient client)
        {
            _client = client;
        }

        public void SendChannelMessage(string message, params object[] args)
        {
            _client.SendMessage(Constants.User.ChannelName, string.Format(message, args));
        }
    }
}