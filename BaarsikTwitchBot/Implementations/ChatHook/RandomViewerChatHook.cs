using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class RandomViewerChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly TwitchApiHelper _apiHelper;

        public RandomViewerChatHook(TwitchClient client, TwitchApiHelper apiHelper)
        {
            _client = client;
            _apiHelper = apiHelper;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Moderators;

        public IList<string> CommandNames { get; } = new List<string> {"randomviewer"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var randomViewer = _apiHelper.GetRandomViewer();
            var chance = 100 / _apiHelper.CurrentViewers.Count;
            _client.SendMessage(chatMessage.Channel, $"Случайный зритель: {randomViewer.DisplayName} - {chance}%");
        }
    }
}