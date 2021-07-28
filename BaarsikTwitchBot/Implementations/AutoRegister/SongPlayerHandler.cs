using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public List<SongRequest> RequestQueue { get; set; } = new();
        public SongRequest CurrentRequest => RequestQueue.FirstOrDefault();
        public TimeSpan CurrentRequestTimeSpan { get; set; } = TimeSpan.Zero;
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
        private async void OnRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
        {
            if (!IsInitialized || !_config.SongRequestManager.Enabled) return;

            var reward = e.RewardRedeemed.Redemption.Reward;
            var requestType = reward.Title == _config.SongRequestManager.RewardTitlePlus
                ? SongRequestType.Plus
                : SongRequestType.Default;

            if (requestType != SongRequestType.Plus && reward.Title != _config.SongRequestManager.RewardTitleDefault)
            {
                return;
            }
            
            var user = _twitchApi.GetFollowerById(e.RewardRedeemed.Redemption.User.Id);
            if (user.IsBannedSongPlayer)
            {
                return;
            }

            var requestUnparsed = e.RewardRedeemed.Redemption.UserInput.Split(' ').FirstOrDefault()?.Trim();

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
                        _clientHelper.SendChannelMessage(SongRequestResources.Reward_VideoNotFound, user.DisplayName);
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
                        _clientHelper.SendChannelMessage(SongRequestResources.Reward_InternalException, user.DisplayName);
                        return;
                    }

                    _logger.Log($"Video parsing error for '{requestUnparsed}': {ex.Message}", LogLevel.Critical);
                    throw;
                }
            }

            if (video == null)
            {
                _logger.Log($"Unexpected error for '{requestUnparsed}': Video has not been parsed", LogLevel.Error);
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_InternalException, user.DisplayName);
                return;
            }

            if (video.Engagement.ViewCount < _config.SongRequestManager.YoutubeMinimumViews)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_InsufficientViews, user.DisplayName, _config.SongRequestManager.YoutubeMinimumViews);
                return;
            }

            if (!video.Duration.HasValue || Math.Abs(video.Duration.Value.TotalMilliseconds) < 0.000001)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_StreamNotSupported, user.DisplayName);
                return;
            }

            var videoDuration = video.Duration.Value;
            if (videoDuration.TotalSeconds < 60)
            {
                var actualDuration = $"{(videoDuration.Hours > 0 ? $"{videoDuration.Hours:00}:" : string.Empty)}{videoDuration.Minutes:00}:{videoDuration.Seconds:00}";
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_LowerThanMinDuration, user.DisplayName, actualDuration);
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
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_MaxDurationExceeded, user.DisplayName, maxDuration, actualDuration);
                return;
            }

            var songInfo = await _dbHelper.GetSongInfoAsync(video.Id) ?? await _dbHelper.UpdateSongInfoAsync(video.Id);
            switch (songInfo.Limitation)
            {
                case SongLimitationType.Banned:
                    _clientHelper.SendChannelMessage(SongRequestResources.Reward_SongIsBanned, user.DisplayName);
                    return;
                case SongLimitationType.Plus when requestType != SongRequestType.Plus:
                    var responseTemplate = _config.SongRequestManager.DisplaySongName
                        ? SongRequestResources.Reward_RequestInPlusOnly_SongName
                        : SongRequestResources.Reward_RequestInPlusOnly_NoSongName;
                    _clientHelper.SendChannelMessage(responseTemplate, user.DisplayName, video.Title);
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
                        _clientHelper.SendChannelMessage(responseTemplate, user.DisplayName, video.Title);
                        return;
                }
            }

            try
            {
                await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
            }
            catch (Exception ex) when (ex is VideoUnavailableException or HttpRequestException)
            {
                _clientHelper.SendChannelMessage(SongRequestResources.Reward_SongUnplayable, user.DisplayName);
                return;
            }
            catch (Exception ex)
            {
                _logger.Log($"Exception occurred: {ex.Message}", LogLevel.Critical);
                throw;
            }

            var songRequest = new SongRequest
            { 
                RewardId = Guid.Parse(reward.Id),
                RedemptionId = Guid.Parse(e.RewardRedeemed.Redemption.Id),
                YoutubeVideo = video,
                RequestType = requestType,
                RequestDate = e.RewardRedeemed.Timestamp,
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

            CurrentRequestTimeSpan = TimeSpan.Zero;

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

            MediaFoundationReader reader;
            var attempts = 0;
            while (true)
            {
                try
                {
                    while (true)
                    {
                        reader = new MediaFoundationReader(audioFileUrl);
                        break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    if (attempts == 5)
                        throw;

                    attempts++;
                    _logger.Log($"UnauthorizedAccessException for {queueItem.YoutubeVideo.Id}: {attempts}/5", LogLevel.Error);
                    Thread.Sleep(50);
                    continue;
                }
                break;
            }

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

            while (waveOut.PlaybackState is PlaybackState.Playing or PlaybackState.Paused)
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
                        CurrentRequestTimeSpan = waveOut.GetPositionTimeSpan();
                        OnCurrentSongTimeSpanUpdated?.Invoke(queueItem, CurrentRequestTimeSpan);
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
                Thread.Sleep(100);
            }

            RequestQueue.RemoveAt(0);
            // ToDo: re-enable this line once custom rewards are created by a bot
            // await _twitchApi.SetRewardRedemptionStatusAsync(queueItem.RewardId.ToString(), queueItem.RedemptionId.ToString(), CustomRewardRedemptionStatus.FULFILLED);
            OnPlayFinished?.Invoke(queueItem, RequestQueue.FirstOrDefault());
            IsPlaying = false;
            IsPaused = false;
            Play();

            await reader.DisposeAsync();
        }
    }
}