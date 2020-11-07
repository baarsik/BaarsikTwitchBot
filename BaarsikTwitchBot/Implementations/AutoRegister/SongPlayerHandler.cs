using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BaarsikTwitchBot.Domain.Enums;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
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

        [VMProtect.BeginVirtualization]
        public void Initialize()
        {
            if (IsInitialized)
                return;

            if (!Directory.Exists("./music"))
            {
                Directory.CreateDirectory("./music");
            }
            else
            {
                var files = Directory.GetFiles("./music");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }

            _twitchApi.OnRewardRedeemed += OnRewardRedeemed;
            IsInitialized = true;
        }

        [VMProtect.BeginVirtualization]
        public async Task BanCurrentSongAsync()
        {
            if (!IsPlayerActive)
                return;

            await _dbHelper.UpdateSongInfoAsync(CurrentRequest.YoutubeVideo.Id, SongLimitationType.Banned);
            IsSkipping = true;
        }

        [VMProtect.BeginVirtualization]
        public async Task LimitSongAsync(SongLimitationType limitType)
        {
            await _dbHelper.UpdateSongInfoAsync(CurrentRequest.YoutubeVideo.Id, limitType);
        }

        [VMProtect.BeginVirtualization]
        private async void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            if (!IsInitialized) return;

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
                    _client.SendMessage(_config.Channel.Name, $"@{e.DisplayName}, видео по данному заказу не найдено");
                    return;
                }
                throw;
            }

            if (Math.Abs(video.Duration.TotalMilliseconds) < 0.000001)
            {
                _client.SendMessage(_config.Channel.Name, $"@{e.DisplayName}, стрим не поддерживается");
                return;
            }

            if (video.Duration.TotalSeconds > _config.SongRequestManager.MaximumLengthInSeconds)
            {
                var minutes = _config.SongRequestManager.MaximumLengthInSeconds / 60;
                var seconds = _config.SongRequestManager.MaximumLengthInSeconds % 60;
                var actualLengthString = $"{(video.Duration.Hours > 0 ? $"{video.Duration.Hours:00}:" : string.Empty)}{video.Duration.Minutes:00}:{video.Duration.Seconds:00}";
                _client.SendMessage(_config.Channel.Name, $"@{e.DisplayName}, видео длительностью более {minutes:00}:{seconds:00} ({actualLengthString})");
                return;
            }

            var songInfo = await _dbHelper.GetSongInfoAsync(video.Id) ?? await _dbHelper.UpdateSongInfoAsync(video.Id);
            switch (songInfo.Limitation)
            {
                case SongLimitationType.Banned:
                    _client.SendMessage(_config.Channel.Name, $"@{e.DisplayName}, данная песня запрещена к заказу стримером");
                    return;
                case SongLimitationType.Plus when requestType != SongRequestType.Plus:
                    _client.SendMessage(_config.Channel.Name, _config.SongRequestManager.DisplaySongName
                        ? $"@{e.DisplayName}, песня '{video.Title}' может быть заказана только наградой '{_config.SongRequestManager.RewardTitlePlus}'"
                        : $"@{e.DisplayName}, данная песня может быть заказана только наградой '{_config.SongRequestManager.RewardTitlePlus}'");
                    return;
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

            _client.SendMessage(_config.Channel.Name,  _config.SongRequestManager.DisplaySongName
                ? $"Добавлена песня '{songRequest.YoutubeVideo.Title}' от @{songRequest.User.DisplayName} (#{_requestQueue.Count})"
                : $"Добавлена песня от @{songRequest.User.DisplayName} (#{_requestQueue.Count})");
        }

        [VMProtect.BeginVirtualization]
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
            var mp3FileName = await GetMp3FileNameAsync(queueItem.YoutubeVideo.Id);

            await using (var reader = new Mp3FileReader(mp3FileName))
            {
                using var waveOut = new WaveOutEvent()
                {
                    Volume = 0.07f
                };
                waveOut.Init(reader);
                waveOut.Play();
                
                if (_config.SongRequestManager.DisplaySongName)
                {
                    _client.SendMessage(_config.Channel.Name, $"Сейчас играет: '{queueItem.YoutubeVideo.Title}' от @{queueItem.User.DisplayName}");
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
            }
            File.Delete(mp3FileName);

            _requestQueue.RemoveAt(0);
            IsPlaying = false;
            IsPaused = false;
            Play();
        }

        [VMProtect.BeginVirtualization]
        private async Task<string> GetMp3FileNameAsync(VideoId videoId)
        {
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
            var streamInfo = streamManifest.GetMuxed().WithHighestBitrate();
            if (streamInfo == null)
                return null;

            await using var memoryStream = new MemoryStream();
            await _youtubeClient.Videos.Streams.CopyToAsync(streamInfo, memoryStream);

            var mp3FileName = $"./music/{videoId}.mp3";
            await using var mfReader = new StreamMediaFoundationReader(memoryStream);
            MediaFoundationEncoder.EncodeToMp3(mfReader, mp3FileName);
            return mp3FileName;
        }
    }
}