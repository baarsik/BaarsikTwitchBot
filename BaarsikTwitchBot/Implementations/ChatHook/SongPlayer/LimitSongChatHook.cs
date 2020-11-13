using System.Collections.Generic;
using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class LimitSongChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public LimitSongChatHook(TwitchClient client, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _client = client;
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
            _client.SendMessage(chatMessage.Channel, string.Format(textTemplate, chatMessage.Username, request.YoutubeVideo.Title, request.User.DisplayName));
        }
    }
}