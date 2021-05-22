using System.Collections.Generic;
using BaarsikTwitchBot.Core.Models;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class SkipSongChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public SkipSongChatHook(TwitchClientHelper clientHelper, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _clientHelper = clientHelper;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => _config.SongRequestManager.Enabled;

        public ChatHookAccessType Access => ChatHookAccessType.Moderators;

        public IList<string> CommandNames { get; } = new List<string> {"skip"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (!_songPlayerHandler.IsPlayerActive)
                return;

            var request = _songPlayerHandler.CurrentRequest;
            if (request.RequestType == SongRequestType.Plus)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.SkipSongChatHook_NonSkippable, request.YoutubeVideo.Title);
                return;
            }

            _songPlayerHandler.IsSkipping = true;

            var textTemplate = _config.SongRequestManager.DisplaySongName
                ? SongRequestResources.SkipSongChatHook_Skipped_SongName
                : SongRequestResources.SkipSongChatHook_Skipped_NoSongName;
            _clientHelper.SendChannelMessage(textTemplate, chatMessage.Username, request.YoutubeVideo.Title, request.User.DisplayName);
        }
    }
}