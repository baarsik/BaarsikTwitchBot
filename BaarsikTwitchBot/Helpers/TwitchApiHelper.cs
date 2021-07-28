using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BaarsikTwitchBot.Domain.Models;
using BaarsikTwitchBot.Extensions;
using BaarsikTwitchBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomRewardRedemptionStatus;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;

namespace BaarsikTwitchBot.Helpers
{
    public class TwitchApiHelper
    {
        private readonly TwitchAPI _twitchApi;
        private readonly JsonConfig _config;
        private readonly DbHelper _dbHelper;
        private readonly ILogger _logger;
        private readonly Random _random;
        private readonly Timer _viewersUpdateTimer;

        public TwitchApiHelper(TwitchAPI twitchApi, JsonConfig config, DbHelper dbHelper, ILogger logger)
        {
            _twitchApi = twitchApi;
            _config = config;
            _dbHelper = dbHelper;
            _logger = logger;
            _random = new Random();

            UpdateFollowerList().Wait();

            _viewersUpdateTimer = new Timer(e => ViewersUpdateTimerTick(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1.5));

            #region PubSub Declaration
            var pubSub = new TwitchPubSub();

            pubSub.OnFollow += PubSubOnChannelFollow;
            pubSub.OnChannelSubscription += PubSubOnChannelSubscription;
            pubSub.OnChannelPointsRewardRedeemed += PubSubOnOnChannelPointsRewardRedeemed;
           
            pubSub.OnListenResponse += (sender, args) =>
            {
                if (!args.Successful)
                    _logger.Log($"Failed to listen! Response: {args.Response.Error} for topic {args.Topic}", LogLevel.Error);
            };
            pubSub.OnPubSubServiceConnected += (sender, args) =>
            {
                _logger.Log("Connected to PubSub", LogLevel.Information);

                pubSub.ListenToVideoPlayback(_config.Channel.Name);
                pubSub.ListenToFollows(StreamerUser.UserId);
                pubSub.ListenToSubscriptions(StreamerUser.UserId);
                pubSub.ListenToChannelPoints(StreamerUser.UserId);
                pubSub.SendTopics(_config.Channel.OAuth);
            };
            pubSub.OnPubSubServiceClosed += (sender, args) =>
            {
                _logger.Log("Disconnected from PubSub. Attempting reconnect", LogLevel.Warning);
                Thread.Sleep(1000);
                pubSub.Connect();
            };
            
            pubSub.Connect();
            #endregion
        }

        ~TwitchApiHelper()
        {
            _viewersUpdateTimer.Dispose();
        }

        public EventHandler<User> OnFollow;
        public EventHandler<OnChannelSubscriptionArgs> OnChannelSubscription;
        public EventHandler<OnChannelPointsRewardRedeemedArgs> OnRewardRedeemed;

        public BotUser StreamerUser { get; private set; }
        public List<BotUser> BotUsers { get; private set; }
        public List<BotUser> CurrentViewers { get; private set; }

        public BotUser GetRandomViewer()
        {
            return CurrentViewers.Any()
                ? CurrentViewers[_random.Next(CurrentViewers.Count)]
                : null;
        }

        public BotUser GetRandomViewer(string excludeUserId)
        {
            var applicableViewers = CurrentViewers.Where(x => x.UserId != excludeUserId).ToList();
            return applicableViewers.Any()
                ? applicableViewers[_random.Next(applicableViewers.Count)]
                : null;
        }

        public BotUser GetFollowerByName(string name)
        {
            return BotUsers.FirstOrDefault(f => string.Equals(f.DisplayName, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task<BotUser> GetUserByNameAsync(string login)
        {
            var userResponse = await _twitchApi.Helix.Users.GetUsersAsync(logins: new List<string> {login});
            var user = userResponse.Users.FirstOrDefault();
            return await _dbHelper.GetUsersQuery().FirstOrDefaultAsync(x => x.UserId == user.Id)
                   ?? user.ToBotUser(false);
        }

        public BotUser GetFollowerByLogin(string login)
        {
            return BotUsers.FirstOrDefault(x => string.Equals(login, x.Login, StringComparison.CurrentCulture));
        }

        public BotUser GetFollowerById(string userId)
        {
            return BotUsers.FirstOrDefault(x => x.UserId == userId);
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        public async Task SetRewardRedemptionStatusAsync(string rewardId, string redemptionId, CustomRewardRedemptionStatus status)
        {
            try
            {
                await _twitchApi.Helix.ChannelPoints.UpdateCustomRewardRedemptionStatus(StreamerUser.UserId, rewardId,
                    new List<string> {redemptionId},
                    new UpdateCustomRewardRedemptionStatusRequest {Status = status}, _config.Channel.OAuth);
            }
            catch (Exception e)
            {
                _logger.Log(e.Message, LogLevel.Error);
            }
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        public async Task<bool> IsTokenValidAsync(string accessToken, string scopes)
        {
            var tokenRoot = await _twitchApi.V5.Root.GetRootAsync(accessToken, _config.OAuth.ClientID);
            var isValid = tokenRoot.Token.Valid
                          && scopes.Split(' ').All(x => tokenRoot.Token.Auth.Scopes.Contains(x));

            return isValid;
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private async Task UpdateFollowerList()
        {
            if (StreamerUser == null)
            {
                var userResponse = await _twitchApi.Helix.Users.GetUsersAsync(logins: new List<string> {_config.Channel.Name});
                var streamerUser = userResponse.Users.First();
                StreamerUser = await _dbHelper.AddUserAsync(streamerUser, false);
            }

            var followers = new List<User>();
            GetUsersFollowsResponse response = null;
            do
            {
                response = await _twitchApi.Helix.Users.GetUsersFollowsAsync(
                    toId: StreamerUser.UserId,
                    first: Constants.Twitch.FollowerRequestLimit,
                    after: response?.Pagination.Cursor);

                if (response != null)
                {
                    var followerIds = response.Follows.Select(x => x.FromUserId).ToList();
                    var usersResponse = await _twitchApi.Helix.Users.GetUsersAsync(ids: followerIds);
                    followers.AddRange(usersResponse.Users);
                }
            } while (response?.Follows.Length == Constants.Twitch.FollowerRequestLimit);

            await _dbHelper.UpdateUsersAsync(followers);

            BotUsers = await _dbHelper.GetUsersQuery().ToListAsync();

            _logger.Log($"Loaded followers list for channel '{StreamerUser.Login}', total followers: {followers.Count}, total bot users: {BotUsers.Count}", LogLevel.Information);
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private async void ViewersUpdateTimerTick()
        {
            var client = new HttpClient();
            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync($"http://tmi.twitch.tv/group/user/{Constants.User.ChannelName}/chatters");
            }
            catch (HttpRequestException e)
            {
                _logger.Log($"Error reaching tmi.twitch.tv: {e.Message}", LogLevel.Error);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var viewersJson = await response.Content.ReadAsStringAsync();
            dynamic jsonObj = JsonConvert.DeserializeObject(viewersJson);
            var viewersNames = new List<string>();
            viewersNames.AddRange(jsonObj.chatters.viewers.ToObject<string[]>());
            viewersNames.AddRange(jsonObj.chatters.vips.ToObject<string[]>());
            viewersNames.AddRange(jsonObj.chatters.moderators.ToObject<string[]>());
            viewersNames.AddRange(jsonObj.chatters.broadcaster.ToObject<string[]>());

            var validFollowerViewerNames = viewersNames
                .Where(name => !_config.Chat.UsersToIgnore.Contains(name.ToLower())
                            && (BotUsers.Any(f => string.Equals(name, f.DisplayName, StringComparison.CurrentCultureIgnoreCase)
                                                  || string.Equals(name, f.Login, StringComparison.CurrentCultureIgnoreCase))
                                || string.Equals(name, Constants.User.ChannelName, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();

            CurrentViewers = BotUsers.Where(x => validFollowerViewerNames.Any(name => string.Equals(name, x.DisplayName, StringComparison.CurrentCultureIgnoreCase))).ToList();
        }

        #region PubSub
        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private async void PubSubOnChannelFollow(object sender, OnFollowArgs e)
        {
            var userResponse = await _twitchApi.Helix.Users.GetUsersAsync(ids: new List<string> { e.UserId });
            var user = userResponse.Users.FirstOrDefault();

            var botUser = await _dbHelper.AddUserAsync(user);
            BotUsers = await _dbHelper.GetUsersQuery().ToListAsync();

            if (botUser.IsBanned)
                return;

            _logger.Log($"New follower at '{StreamerUser.Login}': {(e.Username == e.DisplayName ? e.DisplayName : $"{e.DisplayName} ({e.Username})")}", LogLevel.Information);

            OnFollow?.Invoke(sender, user);
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void PubSubOnChannelSubscription(object sender, OnChannelSubscriptionArgs e)
        {
            var isBanned = BotUsers.Where(x => x.UserId == e.Subscription.UserId).Select(x => x.IsBanned).FirstOrDefault();
            if (isBanned)
                return;

            var subscriberName = e.Subscription.Username == e.Subscription.RecipientDisplayName ? e.Subscription.RecipientDisplayName : $"{e.Subscription.RecipientDisplayName} ({e.Subscription.RecipientName})";
            _logger.Log($"New subscriber at '{StreamerUser.Login}': {subscriberName}", LogLevel.Information);

            OnChannelSubscription?.Invoke(sender, e);
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void PubSubOnOnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
        {
            if (e.RewardRedeemed.Redemption.Status != Constants.RewardStatus.Unfulfilled)
                return;

            var isBanned = BotUsers.Where(x => x.UserId == e.RewardRedeemed.Redemption.User.Id).Select(x => x.IsBanned).FirstOrDefault();
            if (isBanned)
                return;

            _logger.Log($"Reward '{e.RewardRedeemed.Redemption.Reward.Title}' redeemed at '{StreamerUser.Login}' {e.RewardRedeemed.Redemption.User.Login}. Message: \"{e.RewardRedeemed.Redemption.UserInput}\"", LogLevel.Information);
            OnRewardRedeemed?.Invoke(sender, e);
        }
        #endregion
    }
}