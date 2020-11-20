using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class BanUserChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly TwitchApiHelper _apiHelper;
        private readonly DbHelper _dbHelper;

        public BanUserChatHook(TwitchClientHelper clientHelper, TwitchApiHelper apiHelper, DbHelper dbHelper)
        {
            _clientHelper = clientHelper;
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
            _clientHelper.SendChannelMessage(ChatResources.BanUserChatHook_Banned, user.DisplayName);
        }
    }
}