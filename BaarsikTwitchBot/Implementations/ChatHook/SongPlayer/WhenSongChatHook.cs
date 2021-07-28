using System;
using System.Collections.Generic;
using System.Linq;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class WhenSongChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public WhenSongChatHook(TwitchClientHelper clientHelper, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _clientHelper = clientHelper;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => _config.SongRequestManager.Enabled;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; } = new List<string> {"when", "whensong", "когда"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (_songPlayerHandler.IsPaused)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.WhenSongChatHook_Paused, chatMessage.Username);
                return;
            }

            var request = _songPlayerHandler.RequestQueue.FirstOrDefault(x => x.User.UserId == chatMessage.UserId);
            if (request == null)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.WhenSongChatHook_NoRequests, chatMessage.Username);
                return;
            }

            var songIndex = _songPlayerHandler.RequestQueue.IndexOf(request);

            if (songIndex == 0)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.WhenSongChatHook_Now, chatMessage.Username);
                return;
            }

            var totalTime = TimeSpan.Zero;
            for (var i = 1; i < songIndex; i++)
            {
                totalTime += _songPlayerHandler.RequestQueue[songIndex].YoutubeVideo.Duration ?? TimeSpan.Zero;
            }

            totalTime += _songPlayerHandler.CurrentRequest.YoutubeVideo.Duration.Value - _songPlayerHandler.CurrentRequestTimeSpan;
                
            _clientHelper.SendChannelMessage(SongRequestResources.WhenSongChatHook_RequestData, chatMessage.Username, songIndex, $"{totalTime.TotalHours:00}:{totalTime.Minutes:00}:{totalTime.Seconds:00}");
        }
    }
}