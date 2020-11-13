using System.Collections.Generic;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class SongNameChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public SongNameChatHook(TwitchClient client, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _client = client;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => _config.SongRequestManager.DisplaySongName;

        public ChatHookAccessType Access => ChatHookAccessType.Everyone;

        public IList<string> CommandNames { get; set; } = new List<string> {"песня", "трек", "song", "songname"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            var request = _songPlayerHandler.CurrentRequest;
            if (!_songPlayerHandler.IsPlaying || request == null)
            {
                _client.SendMessage(chatMessage.Channel,  SongRequestResources.SongNameChatHook_NoRequests);
                return;
            }

            if (_songPlayerHandler.IsPaused)
            {
                _client.SendMessage(chatMessage.Channel, SongRequestResources.SongNameChatHook_Paused);
                return;
            }

            _client.SendMessage(chatMessage.Channel, string.Format(SongRequestResources.SongNameChatHook_Playing, request.YoutubeVideo.Title, request.User.DisplayName));
        }
    }
}