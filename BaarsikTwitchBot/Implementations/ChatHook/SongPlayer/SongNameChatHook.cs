using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class SongNameChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public SongNameChatHook(TwitchClientHelper clientHelper, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _clientHelper = clientHelper;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => _config.SongRequestManager.Enabled
                                 && _config.SongRequestManager.DisplaySongName;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; set; } = new List<string> {"песня", "трек", "song", "songname"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var request = _songPlayerHandler.CurrentRequest;
            if (!_songPlayerHandler.IsPlaying || request == null)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.SongNameChatHook_NoRequests);
                return;
            }

            if (_songPlayerHandler.IsPaused)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.SongNameChatHook_Paused);
                return;
            }

            _clientHelper.SendChannelMessage(SongRequestResources.SongNameChatHook_Playing, request.YoutubeVideo.Title, request.User.DisplayName);
        }
    }
}