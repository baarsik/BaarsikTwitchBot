using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class RandomViewerChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly TwitchApiHelper _apiHelper;

        public RandomViewerChatHook(TwitchClientHelper clientHelper, TwitchApiHelper apiHelper)
        {
            _clientHelper = clientHelper;
            _apiHelper = apiHelper;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Moderators;

        public IList<string> CommandNames { get; } = new List<string> {"randomviewer"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var randomViewer = _apiHelper.GetRandomViewer();
            var chance = 100 / _apiHelper.CurrentViewers.Count;
            _clientHelper.SendChannelMessage(ChatResources.RandomViewerChatHook_Text, randomViewer.DisplayName, chance);
        }
    }
}