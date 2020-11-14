using System.Collections.Generic;
using System.Linq;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class GetViewersChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly TwitchApiHelper _apiHelper;

        public GetViewersChatHook(TwitchClient client, TwitchApiHelper apiHelper)
        {
            _client = client;
            _apiHelper = apiHelper;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Broadcaster;

        public IList<string> CommandNames { get; } = new List<string> {"getviewers"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var viewerNames = string.Join(", ", _apiHelper.CurrentViewers.Select(x => x.DisplayName));
            var viewerCount = _apiHelper.CurrentViewers.Count;
            _client.SendMessage(chatMessage.Channel, string.Format(ChatResources.GetViewersChatHook_Text, viewerCount, viewerNames));
        }
    }
}