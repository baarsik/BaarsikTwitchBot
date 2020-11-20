using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class FarewellChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly JsonConfig _config;

        public FarewellChatHook(TwitchClientHelper clientHelper, JsonConfig config)
        {
            _clientHelper = clientHelper;
            _config = config;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"ухожу", "gonna", "bye", "cya", "going"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (parameters.Count == 0 || _config.Chat.DisableUnsafeCommands)
            {
                _clientHelper.SendChannelMessage($"{chatMessage.Username} уже уходит. Приятного времени суток {_config.TwitchEmotes.Love}");
            }
            else
            {
                _clientHelper.SendChannelMessage($"{chatMessage.Username} уходит {string.Join(' ', parameters)}. Будем ждать возвращения {_config.TwitchEmotes.Love}");
            }
        }
    }
}