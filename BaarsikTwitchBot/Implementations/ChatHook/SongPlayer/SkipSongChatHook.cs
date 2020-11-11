﻿using System.Collections.Generic;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Implementations.ChatHook.SongPlayer
{
    public class SkipSongChatHook : IChatHook
    {
        private readonly TwitchClient _client;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public SkipSongChatHook(TwitchClient client, SongPlayerHandler songPlayerHandler, JsonConfig config)
        {
            _client = client;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
        }

        public bool IsEnabled => true;

        public ChatHookAccessType Access => ChatHookAccessType.Moderators;

        public IList<string> CommandNames { get; } = new List<string> {"skip"};

        public void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters)
        {
            if (!_songPlayerHandler.IsPlayerActive)
                return;

            var request = _songPlayerHandler.CurrentRequest;
            if (request.RequestType == SongRequestType.Plus)
            {
                _client.SendMessage(chatMessage.Channel, $"Трек '{request.YoutubeVideo.Title}' нельзя скипнуть командой !skip");
                return;
            }

            _songPlayerHandler.IsSkipping = true;

            var text = _config.SongRequestManager.DisplaySongName
                ? $"{chatMessage.Username} скипнул трек '{request.YoutubeVideo.Title}' от @{request.User.DisplayName}"
                : $"{chatMessage.Username} скипнул трек от @{request.User.DisplayName}";
            _client.SendMessage(chatMessage.Channel, text);
        }
    }
}