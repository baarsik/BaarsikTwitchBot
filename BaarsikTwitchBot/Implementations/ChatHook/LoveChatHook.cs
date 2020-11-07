using System;
using System.Collections.Generic;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class LoveChatHook : IChatHook
    {
        private readonly TwitchClient _client;

        public LoveChatHook(TwitchClient client)
        {
            _client = client;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"люблю", "любит", "любовь", "love"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (parameters.Count == 0)
                return;

            var percentage = new Random().Next(0, 100);
            switch (percentage)
            {
                case 0:
                    _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} любит {string.Join(' ', parameters)} на {percentage}%, то есть никак LUL");
                    break;
                case 100:
                    _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} любит {string.Join(' ', parameters)} на {percentage}% - абсолютная любовь baarsiLove");
                    break;
                default:
                    _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} любит {string.Join(' ', parameters)} на {percentage}%");
                    break;
            }
        }
    }
}