using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class BanUserChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly TwitchApiHelper _apiHelper;
        private readonly DbHelper _dbHelper;

        public BanUserChatHook(TwitchClient client, TwitchApiHelper apiHelper, DbHelper dbHelper)
        {
            _client = client;
            _apiHelper = apiHelper;
            _dbHelper = dbHelper;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Broadcaster;

        public IList<string> CommandNames { get; } = new List<string> { "banuser" };

        public async void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (parameters.Count != 1)
                return;

            var userName = parameters[0].Replace("@", "");
            var user = _apiHelper.GetFollowerByName(userName);
            
            if (user == null || user.IsBanned)
                return;

            await _dbHelper.BanUserAsync(user);
            _client.SendMessage(chatMessage.Channel, $"Пользователь {user.DisplayName} забанен и больше не может пользоваться ботом");
        }
    }
}