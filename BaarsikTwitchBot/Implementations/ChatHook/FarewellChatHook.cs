using System.Collections.Generic;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class FarewellChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly JsonConfig _config;

        public FarewellChatHook(TwitchClient client, JsonConfig config)
        {
            _client = client;
            _config = config;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"ухожу", "gonna", "bye", "cya", "going"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (parameters.Count == 0 || _config.Chat.DisableUnsafeCommands)
            {
                _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} уже уходит. Приятного времени суток baarsiLove");
            }
            else
            {
                _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} уходит {string.Join(' ', parameters)}. Будем ждать возвращения baarsiLove");
            }
        }
    }
}