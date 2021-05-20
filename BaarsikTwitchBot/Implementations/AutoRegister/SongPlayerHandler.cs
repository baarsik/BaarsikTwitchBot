using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Resources;
using Microsoft.Extensions.Logging;
using NAudio.Utils;
using NAudio.Wave;
using TwitchLib.Api.Core.Enums;
using TwitchLib.PubSub.Events;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;

namespace BaarsikTwitchBot.Implementations.AutoRegister
{
    public class SongPlayerHandler : IAutoRegister
    {
        private readonly JsonConfig _config;
        private readonly TwitchApiHelper _twitchApi;
        private readonly TwitchClientHelper _clientHelper;
        private readonly DbHelper _dbHelper;
        private readonly ILogger _logger;
        private readonly YoutubeClient _youtubeClient;

        public SongPlayerHandler(JsonConfig config, TwitchApiHelper twitchApi, TwitchClientHelper clientHelper, DbHelper dbHelper, ILogger logger)
        {
            _config = config;
            _twitchApi = twitchApi;
            _clientHelper = clientHelper;
            _dbHelper = dbHelper;
            _logger = logger;
            _youtubeClient = new YoutubeClient();
        }

        public bool IsInitialized { get; private set; }
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public bool IsSkipping { get; set; }
        public float Volume { get; set; }
        public List<SongRequest> RequestQueue { get; set; } = new List<SongRequest>();
        public SongRequest CurrentRequest => RequestQueue.FirstOrDefault();
        public bool IsPlayerActive => IsInitialized && IsPlaying && !IsSkipping && CurrentRequest != null;

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        public void Initialize()
        {
            if (IsInitialized)
                return;

            Volume = _config.SongRequestManager.SoundVolume >= 100
                ? 1f
                : _config.SongRequestManager.SoundVolume / 100f;

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
            if (!IsPlayerActive)
                return;

            await _dbHelper.UpdateSongInfoAsync(CurrentRequest.YoutubeVideo.Id, limitType);
            if (CurrentRequest.RequestType == SongRequestType.Default)
            {
                IsSkipping = true;
            }
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

            var requestUnparsed = e.Message.Split(' ').FirstOrDefault()?.Trim();

            Video video = null;
            var maximumRetries = 2;
            for (var retry = 0; retry <= maximumRetries; retry++)
            {
                try
                {
                    video = await _youtubeClient.Videos.GetAsync(requestUnparsed);
                    break;
                }
                catch (Exception ex)
                {
                    if (ex is VideoUnplayableException or ArgumentException)
                    {
                        _clientHelper.SendChannelMessage(SongRequestResources.Reward_VideoNotFound, e.DisplayName);
                        return;
                    }

                    if (retry < maximumRetries)
                    {
                        if (ex is RequestLimitExceededException)
                        {
                            _logger.Log($"Video request limit exceeded for '{requestUnparsed}' (attempt {retry + 1} out of {maximumRetries + 1}): {ex.Message}", LogLevel.Error);
                            Thread.Sleep(500);
                            continue;
                        }
                    }
                    else if (ex is RequestLimitExceededException)
                    {
                        _logger.Log($"Maximum attempts reached. Cannot not parse video '{requestUnparsed}'", LogLevel.Error);
                        _clientHelper.SendChannelMessage(SongRequestResources.Reward_InternalException, e.DisplayName);
                        return;
                    }

                    _logger.Log($"Video parsing error for '{requestUnparsed}': {ex.Message}", LogLevel.Critical);
                    throw;
                }
            }

            if (video == null)
            {
                _logger.Log($"Unexpected error for '{requestUnparsed}': Video has not been parsed", LogLevel.Error);
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_InternalException, e.DisplayName);
                return;
            }

            if (video.Engagement.ViewCount < _config.SongRequestManager.YoutubeMinimumViews)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_InsufficientViews, e.DisplayName, _config.SongRequestManager.YoutubeMinimumViews);
                return;
            }

            if (!video.Duration.HasValue || Math.Abs(video.Duration.Value.TotalMilliseconds) < 0.000001)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_StreamNotSupported, e.DisplayName);
                return;
            }

            var videoDuration = video.Duration.Value;
            if (videoDuration.TotalSeconds < 60)
            {
                var actualDuration = $"{(videoDuration.Hours > 0 ? $"{videoDuration.Hours:00}:" : string.Empty)}{videoDuration.Minutes:00}:{videoDuration.Seconds:00}";
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_LowerThanMinDuration, e.DisplayName, actualDuration);
                return;
            }

            var maxLengthInSeconds = requestType == SongRequestType.Plus
                ? _config.SongRequestManager.MaximumLengthInSecondsPlus
                : _config.SongRequestManager.MaximumLengthInSeconds;

            if (videoDuration.TotalSeconds > maxLengthInSeconds)
            {
                var minutes = maxLengthInSeconds / 60;
                var seconds = maxLengthInSeconds % 60;
                var maxDuration = $"{minutes:00}:{seconds:00}";
                var actualDuration = $"{(videoDuration.Hours > 0 ? $"{videoDuration.Hours:00}:" : string.Empty)}{videoDuration.Minutes:00}:{videoDuration.Seconds:00}";
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_MaxDurationExceeded, e.DisplayName, maxDuration, actualDuration);
                return;
            }

            var songInfo = await _dbHelper.GetSongInfoAsync(video.Id) ?? await _dbHelper.UpdateSongInfoAsync(video.Id);
            switch (songInfo.Limitation)
            {
                case SongLimitationType.Banned:
                    _clientHelper.SendChannelMessage(SongRequestResources.Reward_SongIsBanned, e.DisplayName);
                    return;
                case SongLimitationType.Plus when requestType != SongRequestType.Plus:
                    var responseTemplate = _config.SongRequestManager.DisplaySongName
                        ? SongRequestResources.Reward_RequestInPlusOnly_SongName
                        : SongRequestResources.Reward_RequestInPlusOnly_NoSongName;
                    _clientHelper.SendChannelMessage(responseTemplate, e.DisplayName, video.Title);
                    return;
            }

            if (RequestQueue.Any(x => x.YoutubeVideo.Id == video.Id))
            {
                switch (requestType)
                {
                    case SongRequestType.Default when !_config.SongRequestManager.AllowDuplicatesDefault:
                    case SongRequestType.Plus when !_config.SongRequestManager.AllowDuplicatesPlus:
                        var responseTemplate = _config.SongRequestManager.DisplaySongName
                            ? SongRequestResources.Reward_SongInQueue_SongName
                            : SongRequestResources.Reward_SongInQueue_NoSongName;
                        _clientHelper.SendChannelMessage(responseTemplate, e.DisplayName, video.Title);
                        return;
                }
            }

            try
            {
                await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
            }
            catch (VideoUnplayableException)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_SongUnplayable, e.DisplayName);
                return;
            }

            var songRequest = new SongRequest
            { 
                RewardId = e.RewardId,
                RedemptionId = e.RedemptionId,
                YoutubeVideo = video,
                RequestType = requestType,
                RequestDate = e.TimeStamp,
                User = user
            };
            RequestQueue.Add(songRequest);
            OnRequestAdded?.Invoke(songRequest);
            Play();

            var textTemplate = _config.SongRequestManager.DisplaySongName
                ? SongRequestResources.Reward_SongAdded_SongName
                : SongRequestResources.Reward_SongAdded_NoSongName;
            _clientHelper.SendChannelMessage(textTemplate, songRequest.YoutubeVideo.Title, songRequest.User.DisplayName, RequestQueue.Count);
        }

        public delegate void SongRequestArgs(SongRequest songRequest);
        public delegate void SongPlayFinishedArgs(SongRequest songRequest, SongRequest nextRequest);
        public delegate void SongTimeSpanUpdatedArgs(SongRequest songRequest, TimeSpan timeSpan);
        public event SongRequestArgs OnPlayStarted;
        public event SongPlayFinishedArgs OnPlayFinished;
        public event SongRequestArgs OnRequestAdded;
        public event SongRequestArgs OnSongPaused;
        public event SongRequestArgs OnSongResumed;
        public event SongRequestArgs OnSongSkipped;
        public event SongTimeSpanUpdatedArgs OnCurrentSongTimeSpanUpdated;

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private async void Play()
        {
            if (IsPlaying)
                return;

            if (RequestQueue.Count == 0)
            {
                IsPlaying = false;
                return;
            }

            IsPlaying = true;
            IsPaused = false;

            var queueItem = RequestQueue.First();
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(queueItem.YoutubeVideo.Id);
            var audioFileUrl = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate().Url;

            await using var reader = new MediaFoundationReader(audioFileUrl);
            await using var channel = new WaveChannel32(reader)
            {
                Volume = Volume,
                PadWithZeroes = false
            };
            using var waveOut = new WaveOutEvent();
            waveOut.Init(channel);
            waveOut.Play();

            if (_config.SongRequestManager.DisplaySongName)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.Announce_CurrentSong, queueItem.YoutubeVideo.Title, queueItem.User.DisplayName);
            }

            OnPlayStarted?.Invoke(queueItem);

            while (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
            {
                if (IsSkipping)
                {
                    OnSongSkipped?.Invoke(queueItem);
                    waveOut.Stop();
                    IsSkipping = false;
                }
                switch (waveOut.PlaybackState)
                {
                    case PlaybackState.Playing:
                        OnCurrentSongTimeSpanUpdated?.Invoke(queueItem, waveOut.GetPositionTimeSpan());
                        if (Math.Abs(Volume - channel.Volume) > 0.001f)
                        {
                            channel.Volume = Volume;
                        }
                        if (IsPaused)
                        {
                            OnSongPaused?.Invoke(queueItem);
                            waveOut.Pause();
                        }
                        break;
                    case PlaybackState.Paused:
                        if (!IsPaused)
                        {
                            OnSongResumed?.Invoke(queueItem);
                            waveOut.Play();
                        }
                        break;
                    case PlaybackState.Stopped:
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(waveOut.PlaybackState));
                }
                System.Threading.Thread.Sleep(100);
            }

            RequestQueue.RemoveAt(0);
            await _twitchApi.SetRewardRedemptionStatusAsync(queueItem.RewardId.ToString(), queueItem.RedemptionId.ToString(), CustomRewardRedemptionStatus.FULFILLED);
            OnPlayFinished?.Invoke(queueItem, RequestQueue.FirstOrDefault());
            IsPlaying = false;
            IsPaused = false;
            Play();
        }
    }
}