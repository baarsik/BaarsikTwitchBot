using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class LurkChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;

        public LurkChatHook(TwitchClientHelper clientHelper)
        {
            _clientHelper = clientHelper;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"фон", "отошел", "афк", "afk", "lurk"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            _clientHelper.SendChannelMessage(ChatResources.LurkChatHook_Lurking, chatMessage.Username);
        }
    }
}