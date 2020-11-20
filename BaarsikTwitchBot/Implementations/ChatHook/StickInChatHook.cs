using System;
using System.Collections.Generic;
using BaarsikTwitchBot.Domain.Models;
using BaarsikTwitchBot.Extensions;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class StickInChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly TwitchApiHelper _apiHelper;
        private readonly Random _random;

        public StickInChatHook(TwitchClientHelper clientHelper, TwitchApiHelper apiHelper)
        {
            _clientHelper = clientHelper;
            _apiHelper = apiHelper;
            _random = new Random();
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"присунуть", "stickin", "bang"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var user = GetTargetUser(chatMessage.UserId, parameters);
            if (user == null)
                return;

            var length = _random.NextGaussian(13.2d, 2.7d);
            _clientHelper.SendChannelMessage(ChatResources.StickInChatHook_Banged, chatMessage.Username, user.DisplayName, length);
        }

        private BotUser GetTargetUser(string callingUserId, IList<string> parameters)
        {
            if (parameters.Count == 1)
            {
                var userName = parameters[0].Replace("@", "");
                var user = _apiHelper.GetFollowerByName(userName);
                if (user == null || user.UserId != callingUserId)
                {
                    return user;
                }
            }

            return _apiHelper.GetRandomViewer(callingUserId);
        }
    }
}