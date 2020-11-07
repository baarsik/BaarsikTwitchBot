using BaarsikTwitchBot.Domain.Models;
using TwitchLib.Api.Helix.Models.Users;

namespace BaarsikTwitchBot.Extensions
{
    public static class UserExtensions
    {
        public static BotUser ToBotUser(this User user, bool isFollower = true)
            => new BotUser
            {
                UserId = user.Id,
                Login = user.Login,
                DisplayName = user.DisplayName,
                IsFollower = isFollower
            };
    }
}