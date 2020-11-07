using System.Collections.Generic;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class BanSongChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly SongPlayerHandler _songPlayerHandler;

        public BanSongChatHook(TwitchClient client, SongPlayerHandler songPlayerHandler)
        {
            _client = client;
            _songPlayerHandler = songPlayerHandler;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Broadcaster;

        public IList<string> CommandNames { get; } = new List<string> {"bansong"};

        public async void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (!_songPlayerHandler.IsPlayerActive)
                return;

            var request = _songPlayerHandler.CurrentRequest;
            await _songPlayerHandler.BanCurrentSongAsync();
            _client.SendMessage(chatMessage.Channel, $"{chatMessage.Username} забанил трек '{request.YoutubeVideo.Title}' от @{request.User.DisplayName}");
        }
    }
}