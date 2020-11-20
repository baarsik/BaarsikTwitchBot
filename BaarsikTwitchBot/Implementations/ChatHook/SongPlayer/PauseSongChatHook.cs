using System.Collections.Generic;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class PauseSongChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public PauseSongChatHook(TwitchClientHelper clientHelper, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _clientHelper = clientHelper;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => _config.SongRequestManager.Enabled;

        public ChatHookAccessType Access => ChatHookAccessType.Moderators;

        public IList<string> CommandNames { get; } = new List<string> {"pause"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (!_songPlayerHandler.IsPlayerActive)
                return;

            var request = _songPlayerHandler.CurrentRequest;
            _songPlayerHandler.IsPaused = !_songPlayerHandler.IsPaused;

            var textTemplate = _songPlayerHandler.IsPaused switch
            {
                true when _config.SongRequestManager.DisplaySongName => SongRequestResources.PauseSongChatHook_Pause_SongName,
                true => SongRequestResources.PauseSongChatHook_Pause_NoSongName,
                false when _config.SongRequestManager.DisplaySongName => SongRequestResources.PauseSongChatHook_Resume_SongName,
                false => SongRequestResources.PauseSongChatHook_Resume_NoSongName,
            };
            _clientHelper.SendChannelMessage(textTemplate, chatMessage.Username, request.YoutubeVideo.Title, request.User.DisplayName);
        }
    }
}