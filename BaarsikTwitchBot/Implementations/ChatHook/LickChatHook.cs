using System;
using System.Collections.Generic;
using System.Linq;
using BaarsikTwitchBot.Domain.Models;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook
{
    public class LickChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly TwitchApiHelper _apiHelper;
        private readonly JsonConfig _config;
        private readonly DbHelper _dbHelper;
        private readonly Random _random;

        public LickChatHook(TwitchClient client, TwitchApiHelper apiHelper, JsonConfig config, DbHelper dbHelper)
        {
            _client = client;
            _apiHelper = apiHelper;
            _config = config;
            _dbHelper = dbHelper;
            _random = new Random();
        }

        public bool IsEnabled => true;
        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"лизь", "lick"};

        public async void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var user = GetTargetUser(parameters);
            if (user == null)
                return;

            var lickTickets = new List<int>
            {
                100000, 70000, 50000, 35000, 18000, 11000, 7000, 4000, 2100, 1200, 700, 400, 230, 120, 60, 40, 20, 10, 8, 6, 4, 2, 1
            };
            var random = _random.Next(1, lickTickets.Sum() + 1);
            var licks = 0;
            do
            {
                random -= lickTickets[licks];
                licks++;
            } while (random > 0 && licks < lickTickets.Count);

            var timesString = (licks % 10 == 2 || licks % 10 == 3 || licks % 10 == 4) && licks != 12 && licks != 13 && licks != 14 ? "раза" : "раз";
            var nameString = chatMessage.UserId == user.UserId ? "себя" : user.DisplayName;
            var emoteString = licks >= 10 ? $"{_config.TwitchEmotes.PogChamp} {_config.TwitchEmotes.Gasm}" : licks >= 7 ? _config.TwitchEmotes.Gasm : string.Empty;
            _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} облизывает {nameString} {licks} {timesString} {emoteString}");

            user.Statistics.LicksReceived += (uint)licks;
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