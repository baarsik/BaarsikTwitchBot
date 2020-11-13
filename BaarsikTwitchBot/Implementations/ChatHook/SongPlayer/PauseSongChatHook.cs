using System.Collections.Generic;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class PauseSongChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public PauseSongChatHook(TwitchClient client, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _client = client;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => true;

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
            var text = string.Format(textTemplate, chatMessage.Username, request.YoutubeVideo.Title, request.User.DisplayName);
            _client.SendMessage(chatMessage.Channel, text);
        }
    }
}