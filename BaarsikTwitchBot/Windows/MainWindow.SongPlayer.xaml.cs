using System;
using System.Linq;
using BaarsikTwitchBot.Core.Messages;
using BaarsikTwitchBot.Core.Models;
using BaarsikTwitchBot.Messaging.Sender;
using BaarsikTwitchBot.Models;

namespace BaarsikTwitchBot.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public SongPlayerViewModel SongPlayer { get; set; }

        private readonly IMessageSender _messageSender;

        private void InitSongPlayer()
        {
            SongPlayer = new SongPlayerViewModel
            {
                CurrentRequest = _songPlayerHandler.CurrentRequest,
                IsPlaying = _songPlayerHandler.IsPlaying,
                Queue = _songPlayerHandler.RequestQueue,
                Volume = (int) (_songPlayerHandler.Volume * 1000)
            };
            _songPlayerHandler.OnPlayStarted += SongPlayerOnPlayStarted;
            _songPlayerHandler.OnSongPaused += SongPlayerOnPlayPaused;
            _songPlayerHandler.OnPlayFinished += SongPlayerOnPlayFinished;
            _songPlayerHandler.OnRequestAdded += SongPlayerOnRequestAdded;
            _songPlayerHandler.OnCurrentSongTimeSpanUpdated += SongPlayerOnCurrentSongTimeSpanUpdated;
            SongPlayer.OnVolumeChanged += OnSongPlayerVolumeChanged;
            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnCurrentSongTimeSpanUpdated(SongRequest songRequest, TimeSpan timeSpan)
        {
            SongPlayer.CurrentRequestTimeSpan = timeSpan;

            _messageSender.SendMessage(new SongPlayCurrentSongTimeSpanUpdatedMessage()
                {CurrentRequestTimeSpan = SongPlayer.CurrentRequestTimeSpan});

            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnRequestAdded(SongRequest songRequest)
        {
            SongPlayer.Queue = _songPlayerHandler.RequestQueue.ToList();

            _messageSender.SendMessage(new SongPlayRequestAddedMessage() {Queue = SongPlayer.Queue});

            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnPlayFinished(SongRequest songRequest, SongRequest nextRequest)
        {
            SongPlayer.Queue = _songPlayerHandler.RequestQueue.ToList();
            SongPlayer.CurrentRequest = nextRequest;
            SongPlayer.CurrentRequestTimeSpan = TimeSpan.Zero;
            SongPlayer.IsPlaying = false;

            _messageSender.SendMessage(new SongPlayFinishedMessage()
            {
                Queue = SongPlayer.Queue,
                CurrentRequest = SongPlayer.CurrentRequest,
                CurrentRequestTimeSpan = SongPlayer.CurrentRequestTimeSpan,
                IsPlaying = SongPlayer.IsPlaying
            });

            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnPlayStarted(SongRequest songRequest)
        {
            SongPlayer.Queue = _songPlayerHandler.RequestQueue.ToList();
            SongPlayer.CurrentRequest = songRequest;
            SongPlayer.CurrentRequestTimeSpan = TimeSpan.Zero;
            SongPlayer.IsPlaying = true;

            _messageSender.SendMessage(new SongPlayStartedMessage()
            {
                Queue = SongPlayer.Queue,
                CurrentRequest = SongPlayer.CurrentRequest,
                CurrentRequestTimeSpan = SongPlayer.CurrentRequestTimeSpan,
                IsPlaying = SongPlayer.IsPlaying
            });

            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnPlayPaused(SongRequest songRequest)
        {
            SongPlayer.IsPlaying = false;

            _messageSender.SendMessage(new SongPlayPausedMessage() {IsPlaying = false});

            OnPropertyChanged(nameof(SongPlayer));
        }

        private void OnSongPlayerVolumeChanged(int volume)
        {
            if (volume > 1000) volume = 1000;
            else if (volume < 0) volume = 0;
            _songPlayerHandler.Volume = volume / 1000f;

            _messageSender.SendMessage(new SongPlayVolumeChange() {Volume = _songPlayerHandler.Volume});
        }
    }
}