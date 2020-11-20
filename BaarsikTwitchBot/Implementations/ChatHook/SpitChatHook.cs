using System.Collections.Generic;
using BaarsikTwitchBot.Domain.Models;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class SpitChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly TwitchApiHelper _apiHelper;
        private readonly JsonConfig _config;
        private readonly DbHelper _dbHelper;

        public SpitChatHook(TwitchClientHelper clientHelper, TwitchApiHelper apiHelper, JsonConfig config, DbHelper dbHelper)
        {
            _clientHelper = clientHelper;
            _apiHelper = apiHelper;
            _config = config;
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
                ? string.Format(ChatResources.SpitChatHook_SelfSpit, chatMessage.Username, _config.TwitchEmotes.LUL)
                : string.Format(ChatResources.SpitChatHook_Spit, chatMessage.Username, user.DisplayName);

            _clientHelper.SendChannelMessage(message);

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