using System;
using System.Linq;
using BaarsikTwitchBot.Models;

namespace BaarsikTwitchBot.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public SongPlayerViewModel SongPlayer { get; set; }

        private void InitSongPlayer()
        {
            SongPlayer = new SongPlayerViewModel
            {
                CurrentRequest = _songPlayerHandler.CurrentRequest,
                IsPlaying = _songPlayerHandler.IsPlaying,
                Queue = _songPlayerHandler.RequestQueue,
                Volume = (int)(_songPlayerHandler.Volume * 1000)
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
            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnRequestAdded(SongRequest songRequest)
        {
            SongPlayer.Queue = _songPlayerHandler.RequestQueue.ToList();
            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnPlayFinished(SongRequest songRequest, SongRequest nextRequest)
        {
            SongPlayer.Queue = _songPlayerHandler.RequestQueue.ToList();
            SongPlayer.CurrentRequest = nextRequest;
            SongPlayer.CurrentRequestTimeSpan = TimeSpan.Zero;
            SongPlayer.IsPlaying = false;
            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnPlayStarted(SongRequest songRequest)
        {
            SongPlayer.Queue = _songPlayerHandler.RequestQueue.ToList();
            SongPlayer.CurrentRequest = songRequest;
            SongPlayer.CurrentRequestTimeSpan = TimeSpan.Zero;
            SongPlayer.IsPlaying = true;
            OnPropertyChanged(nameof(SongPlayer));
        }

        private void SongPlayerOnPlayPaused(SongRequest songRequest)
        {
            SongPlayer.IsPlaying = false;
            OnPropertyChanged(nameof(SongPlayer));
        }

        private void OnSongPlayerVolumeChanged(int volume)
        {
            if (volume > 1000) volume = 1000;
            else if (volume < 0) volume = 0;
            _songPlayerHandler.Volume = volume / 1000f;
        }
    }
}
