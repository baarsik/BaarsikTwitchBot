using System;
using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class LoveChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly JsonConfig _config;

        public LoveChatHook(TwitchClientHelper clientHelper, JsonConfig config)
        {
            _clientHelper = clientHelper;
            _config = config;
        }

        public bool IsEnabled => !_config.Chat.DisableUnsafeCommands;

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
                    _clientHelper.SendChannelMessage($"{chatMessage.Username} любит {string.Join(' ', parameters)} на {percentage}%, то есть никак {_config.TwitchEmotes.LUL}");
                    break;
                case 100:
                    _clientHelper.SendChannelMessage($"{chatMessage.Username} любит {string.Join(' ', parameters)} на {percentage}% - абсолютная любовь {_config.TwitchEmotes.Love}");
                    break;
                default:
                    _clientHelper.SendChannelMessage($"{chatMessage.Username} любит {string.Join(' ', parameters)} на {percentage}%");
                    break;
            }
        }
    }
}