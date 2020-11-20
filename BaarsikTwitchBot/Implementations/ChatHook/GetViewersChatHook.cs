using System.Collections.Generic;
using System.Linq;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class GetViewersChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly TwitchApiHelper _apiHelper;

        public GetViewersChatHook(TwitchClientHelper clientHelper, TwitchApiHelper apiHelper)
        {
            _clientHelper = clientHelper;
            _apiHelper = apiHelper;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Broadcaster;

        public IList<string> CommandNames { get; } = new List<string> {"getviewers"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var viewerNames = string.Join(", ", _apiHelper.CurrentViewers.Select(x => x.DisplayName));
            var viewerCount = _apiHelper.CurrentViewers.Count;
            _clientHelper.SendChannelMessage(ChatResources.GetViewersChatHook_Text, viewerCount, viewerNames);
        }
    }
}