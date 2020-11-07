using System.Collections.Generic;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class LurkChatHook : IChatHook
    {
        private readonly TwitchClient _client;

        public LurkChatHook(TwitchClient client)
        {
            _client = client;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"фон", "отошел", "афк", "afk", "lurk"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} теперь смотрит стрим на фоне");
        }
    }
}