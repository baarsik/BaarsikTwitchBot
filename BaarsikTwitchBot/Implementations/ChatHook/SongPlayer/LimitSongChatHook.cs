using System.Collections.Generic;
using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class LimitSongChatHook : IChatHook
    {
        private readonly TwitchClientHelper _clientHelper;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public LimitSongChatHook(TwitchClientHelper clientHelper, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _clientHelper = clientHelper;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => _config.SongRequestManager.Enabled;

        public ChatHookAccessType Access => ChatHookAccessType.Broadcaster;

        public IList<string> CommandNames { get; } = new List<string> {"limitsong"};

        public async void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (!_songPlayerHandler.IsPlayerActive)
                return;

            var request = _songPlayerHandler.CurrentRequest;
            await _songPlayerHandler.LimitSongAsync(SongLimitationType.Plus);

            var textTemplate = _config.SongRequestManager.DisplaySongName
                ? SongRequestResources.LimitSongChatHook_Success_SongName
                : SongRequestResources.LimitSongChatHook_Success_NoSongName;
            _clientHelper.SendChannelMessage(textTemplate, chatMessage.Username, request.YoutubeVideo.Title, request.User.DisplayName);
        }
    }
}