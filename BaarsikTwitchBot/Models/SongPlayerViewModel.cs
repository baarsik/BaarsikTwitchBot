using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using BaarsikTwitchBot.Annotations;

namespace BaarsikTwitchBot.Models
{
    public class SongPlayerViewModel : INotifyPropertyChanged
    {
        private SongRequest _currentRequest;
        public SongRequest CurrentRequest
        {
            get => _currentRequest;
            set
            {
                _currentRequest = value;
                OnPropertyChanged(nameof(CurrentRequest));
                OnPropertyChanged(nameof(CurrentRequestThumbnailUrl));
                OnPropertyChanged(nameof(Visibility));
            }
        }

        private TimeSpan? _currentRequestTimeSpan;
        public TimeSpan? CurrentRequestTimeSpan
        {
            get => _currentRequestTimeSpan;
            set
            {
                _currentRequestTimeSpan = value;
                OnPropertyChanged(nameof(CurrentRequestTimeSpan));
                OnPropertyChanged(nameof(CurrentRequestProgress));
            }
        }

        public int CurrentRequestProgress
        {
            get
            {
                if (!CurrentRequestTimeSpan.HasValue || CurrentRequest == null)
                {
                    return 0;
                }

                return (int) (CurrentRequestTimeSpan.Value.TotalMilliseconds / CurrentRequest.YoutubeVideo.Duration.Value.TotalMilliseconds * 10000);
            }
        }

        public string CurrentRequestThumbnailUrl
        {
            get
            {
                if (CurrentRequest == null)
                {
                    return string.Empty;
                }

                return CurrentRequest.YoutubeVideo.Thumbnails.OrderBy(x => x.Resolution.Width).Select(x => x.Url).FirstOrDefault();
            }
        }

        private int _volume;
        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged(nameof(Volume));
                OnVolumeChanged?.Invoke(value);
            }
        }

        public SongRequest NextRequest => Queue.FirstOrDefault(x => x.RewardId != _currentRequest?.RewardId);

        private IList<SongRequest> _queue;
        public IList<SongRequest> Queue
        {
            get => _queue;
            set
            {
                _queue = value;
                OnPropertyChanged(nameof(Queue));
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(nameof(IsPlaying)); }
        }

        public Visibility Visibility => CurrentRequest != null ? Visibility.Visible : Visibility.Collapsed;

        public delegate void VolumeChangedArgs(int volume);
        public event VolumeChangedArgs OnVolumeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}