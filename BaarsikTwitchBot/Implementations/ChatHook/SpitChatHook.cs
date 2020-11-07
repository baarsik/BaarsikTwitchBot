using System.Collections.Generic;
using BaarsikTwitchBot.Domain.Models;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class SpitChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly TwitchApiHelper _apiHelper;
        private readonly DbHelper _dbHelper;

        public SpitChatHook(TwitchClient client, TwitchApiHelper apiHelper, DbHelper dbHelper)
        {
            _client = client;
            _apiHelper = apiHelper;
            _dbHelper = dbHelper;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"хатьфу", "плюнуть", "плевок", "spit"};

        public async void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var user = GetTargetUser(parameters);
            if (user == null)
                return;

            var message = chatMessage.UserId == user.UserId
                ? $"{chatMessage.Username} плюет себе в лицо LUL"
                : $"{chatMessage.Username} плюет в {user.DisplayName}";

            _client.SendMessage(chatMessage.Channel, message);

            user.Statistics.LicksReceived++;
            await _dbHelper.UpdateUserAsync(user);
        }

        private BotUser GetTargetUser(IList<string> parameters)
        {
            if (parameters.Count == 1)
            {
                var userName = parameters[0].Replace("@", "");
                return _apiHelper.GetFollowerByName(userName);
            }

            return _apiHelper.GetRandomViewer();
        }
    }
}