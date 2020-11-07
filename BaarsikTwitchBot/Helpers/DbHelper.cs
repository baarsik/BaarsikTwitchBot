using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaarsikTwitchBot.Domain;
using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Domain.Models;
using BaarsikTwitchBot.Extensions;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api.Helix.Models.Users;
using YoutubeExplode.Videos;

namespace BaarsikTwitchBot.Helpers
{
    public class DbHelper
    {
        private readonly ApplicationContext _dbContext;

        public DbHelper(ApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Users
        public IQueryable<BotUser> GetUsersQuery()
        {
            return _dbContext.Users.Include(x => x.Statistics);
        }

        public async Task<BotUser> AddUserAsync(User user, bool isFollower = true)
        {
            var botUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (botUser != null)
            {
                botUser.IsFollower = isFollower;
                return botUser;
            }

            botUser = user.ToBotUser(isFollower);
            _dbContext.Users.Add(botUser);
            await _dbContext.SaveChangesAsync();
            return botUser;
        }

        public async Task UpdateUsersAsync(IList<User> followers)
        {
            var currentUsers = await _dbContext.Users.ToListAsync();
            var lostFollowers = currentUsers.Where(x => x.IsFollower && followers.All(f => f.Id != x.UserId)).ToList();
            lostFollowers.ForEach(x => x.IsFollower = false);
            _dbContext.Users.UpdateRange(lostFollowers);

            var newFollowers = followers
                .Where(f => !currentUsers.Select(x => x.UserId).Contains(f.Id))
                .Select(f => f.ToBotUser())
                .ToList();
            _dbContext.Users.AddRange(newFollowers);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(BotUser user)
        {
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUsersAsync(IList<BotUser> users)
        {
            _dbContext.UpdateRange(users);
            await _dbContext.SaveChangesAsync();
        }

        public async Task BanUserAsync(BotUser user)
        {
            user.IsBanned = true;
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Song Info
        public async Task<SongInfo> GetSongInfoAsync(string videoId)
        {
            return await _dbContext.SongInfo.FirstOrDefaultAsync(x => x.VideoId == videoId);
        }

        public async Task<SongInfo> UpdateSongInfoAsync(string videoId, SongLimitationType limitationType = SongLimitationType.Default)
        {
            var songInfo = await _dbContext.SongInfo.FirstOrDefaultAsync(x => x.VideoId == videoId);
            if (songInfo != null)
            {
                songInfo.Limitation = limitationType;
                _dbContext.SongInfo.Update(songInfo);
            }
            else
            {
                songInfo = new SongInfo
                {
                    VideoId = videoId,
                    Limitation = limitationType
                };
                _dbContext.SongInfo.Add(songInfo);
            }
            await _dbContext.SaveChangesAsync();
            return songInfo;
        }
        #endregion
    }
}