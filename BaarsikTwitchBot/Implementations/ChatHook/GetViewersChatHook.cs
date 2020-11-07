using System.Collections.Generic;
using System.Linq;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
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
            _client.SendMessage(chatMessage.Channel, $"Зрители ({_apiHelper.CurrentViewers.Count}): {string.Join(", ", _apiHelper.CurrentViewers.Select(x => x.DisplayName))}");
        }
    }
}