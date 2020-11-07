using System.Collections.Generic;
using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
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

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Broadcaster;

        public IList<string> CommandNames { get; } = new List<string> {"limitsong"};

        public async void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (!_songPlayerHandler.IsPlayerActive)
                return;

            var request = _songPlayerHandler.CurrentRequest;
            await _songPlayerHandler.LimitSongAsync(SongLimitationType.Plus);
            _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} перевел трек '{request.YoutubeVideo.Title}' в награду '{_config.SongRequestManager.RewardTitlePlus}'");
        }
    }
}