using System;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;

namespace BaarsikTwitchBot.Helpers
{
    public class TwitchClientHelper
    {
        private readonly TwitchClient _twitchClient;
        private readonly ILogger _logger;

        public TwitchClientHelper(TwitchClient twitchClient, ILogger logger)
        {
            _twitchClient = twitchClient;
            _logger = logger;
        }

        public void SendChannelMessage(string message, params object[] args)
        {
            try
            {
                _twitchClient.SendMessage(Constants.User.ChannelName, string.Format(message, args));
            }
            catch (Exception e)
            {
                _logger.Log(e.Message, LogLevel.Critical);
                throw;
            }
        }
    }
}