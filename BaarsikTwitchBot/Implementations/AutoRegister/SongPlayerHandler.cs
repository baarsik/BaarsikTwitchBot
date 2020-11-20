using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using NAudio.Wave;
using TwitchLib.Client;
using TwitchLib.PubSub.Events;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace BaarsikTwitchBot.Implementations.AutoRegister
{
    public class SongPlayerHandler : IAutoRegister
    {
        private readonly JsonConfig _config;
        private readonly TwitchApiHelper _twitchApi;
        private readonly TwitchClient _client;
        private readonly DbHelper _dbHelper;
        private readonly YoutubeClient _youtubeClient;
        private readonly List<SongRequest> _requestQueue = new List<SongRequest>();

        public SongPlayerHandler(JsonConfig config, TwitchApiHelper twitchApi, TwitchClient client, DbHelper dbHelper)
        {
            _config = config;
            _twitchApi = twitchApi;
            _client = client;
            _dbHelper = dbHelper;
            _youtubeClient = new YoutubeClient();
        }

        public bool IsInitialized { get; private set; }
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public bool IsSkipping { get; set; }
        public SongRequest CurrentRequest => _requestQueue.FirstOrDefault();
        public bool IsPlayerActive => IsInitialized && IsPlaying && !IsSkipping && CurrentRequest != null;

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        public void Initialize()
        {
            if (IsInitialized)
                return;

            _twitchApi.OnRewardRedeemed += OnRewardRedeemed;
            IsInitialized = true;
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        public async Task BanCurrentSongAsync()
        {
            if (!IsPlayerActive)
                return;

            await _dbHelper.UpdateSongInfoAsync(CurrentRequest.YoutubeVideo.Id, SongLimitationType.Banned);
            IsSkipping = true;
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        public async Task LimitSongAsync(SongLimitationType limitType)
        {
            await _dbHelper.UpdateSongInfoAsync(CurrentRequest.YoutubeVideo.Id, limitType);
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private async void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            if (!IsInitialized || !_config.SongRequestManager.Enabled) return;

            var requestType = e.RewardTitle == _config.SongRequestManager.RewardTitlePlus
                ? SongRequestType.Plus
                : SongRequestType.Default;

            if (requestType != SongRequestType.Plus && e.RewardTitle != _config.SongRequestManager.RewardTitleDefault)
            {
                return;
            }

            var user = _twitchApi.GetFollowerByLogin(e.Login) ?? await _twitchApi.GetUserByNameAsync(e.Login);
            if (user.IsBannedSongPlayer)
            {
                return;
            }

            Video video;
            try
            {
                video = await _youtubeClient.Videos.GetAsync(e.Message);
            }
            catch (Exception ex)
            {
                if (ex is VideoUnplayableException || ex is ArgumentException)
                {
                    _client.SendMessage(_config.Channel.Name, string.Format(SongRequestResources.Reward_VideoNotFound, e.DisplayName));
                    return;
                }
                throw;
            }

            if (Math.Abs(video.Duration.TotalMilliseconds) < 0.000001)
            {
                _client.SendMessage(_config.Channel.Name, string.Format(SongRequestResources.Reward_StreamNotSupported, e.DisplayName));
                return;
            }

            if (video.Duration.TotalSeconds > _config.SongRequestManager.MaximumLengthInSeconds)
            {
                var minutes = _config.SongRequestManager.MaximumLengthInSeconds / 60;
                var seconds = _config.SongRequestManager.MaximumLengthInSeconds % 60;
                var maxDuration = $"{minutes:00}:{seconds:00}";
                var actualDuration = $"{(video.Duration.Hours > 0 ? $"{video.Duration.Hours:00}:" : string.Empty)}{video.Duration.Minutes:00}:{video.Duration.Seconds:00}";
                _client.SendMessage(_config.Channel.Name, string.Format(SongRequestResources.Reward_MaxDurationExceeded, e.DisplayName, maxDuration, actualDuration));
                return;
            }

            var songInfo = await _dbHelper.GetSongInfoAsync(video.Id) ?? await _dbHelper.UpdateSongInfoAsync(video.Id);
            switch (songInfo.Limitation)
            {
                case SongLimitationType.Banned:
                    _client.SendMessage(_config.Channel.Name, string.Format(SongRequestResources.Reward_SongIsBanned, e.DisplayName));
                    return;
                case SongLimitationType.Plus when requestType != SongRequestType.Plus:
                    var responseTemplate = _config.SongRequestManager.DisplaySongName ? SongRequestResources.Reward_RequestInPlusOnly_SongName : SongRequestResources.Reward_RequestInPlusOnly_NoSongName;
                    _client.SendMessage(_config.Channel.Name, string.Format(responseTemplate, e.DisplayName, video.Title));
                    return;
            }

            if (_requestQueue.Any(x => x.YoutubeVideo.Id == video.Id))
            {
                switch (requestType)
                {
                    case SongRequestType.Default when !_config.SongRequestManager.AllowDuplicatesDefault:
                    case SongRequestType.Plus when !_config.SongRequestManager.AllowDuplicatesPlus:
                        var responseTemplate = _config.SongRequestManager.DisplaySongName ? SongRequestResources.Reward_SongInQueue_SongName : SongRequestResources.Reward_SongInQueue_NoSongName;
                        _client.SendMessage(_config.Channel.Name, string.Format(responseTemplate, e.DisplayName, video.Title));
                        return;
                }
            }

            var songRequest = new SongRequest
            { 
                RewardId = e.RewardId,
                YoutubeVideo = video,
                RequestType = requestType,
                RequestDate = e.TimeStamp,
                User = user
            };
            _requestQueue.Add(songRequest);
            Play();

            var textTemplate = _config.SongRequestManager.DisplaySongName ? SongRequestResources.Reward_SongAdded_SongName : SongRequestResources.Reward_SongAdded_NoSongName;
            _client.SendMessage(_config.Channel.Name,  string.Format(textTemplate, songRequest.YoutubeVideo.Title, songRequest.User.DisplayName, _requestQueue.Count));
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private async void Play()
        {
            if (IsPlaying)
                return;

            if (_requestQueue.Count == 0)
            {
                IsPlaying = false;
                return;
            }

            IsPlaying = true;
            IsPaused = false;

            var queueItem = _requestQueue.First();
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(queueItem.YoutubeVideo.Id);
            var audioFileUrl = streamManifest.GetAudioOnly().WithHighestBitrate().Url;

            await using var reader = new MediaFoundationReader(audioFileUrl);
            using var waveOut = new WaveOutEvent
            {
                Volume = 0.07f
            };
            waveOut.Init(reader);
            waveOut.Play();

            if (_config.SongRequestManager.DisplaySongName)
            {
                _client.SendMessage(_config.Channel.Name, string.Format(SongRequestResources.Announce_CurrentSong, queueItem.YoutubeVideo.Title, queueItem.User.DisplayName));
            }

            while (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
            {
                switch (waveOut.PlaybackState)
                {
                    case PlaybackState.Playing:
                        if (IsSkipping)
                        {
                            waveOut.Stop();
                            IsSkipping = false;
                            IsPaused = false;
                        }
                        else if (IsPaused)
                        {
                            waveOut.Pause();
                        }
                        break;
                    case PlaybackState.Paused:
                        if (!IsPaused)
                        {
                            waveOut.Play();
                        }
                        break;
                    case PlaybackState.Stopped:
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(waveOut.PlaybackState));
                }
                System.Threading.Thread.Sleep(100);
            }

            _requestQueue.RemoveAt(0);
            IsPlaying = false;
            IsPaused = false;
            Play();
        }
    }
}