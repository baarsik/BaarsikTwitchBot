using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaarsikTwitchBot.Annotations;

namespace BaarsikTwitchBot.Models
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private int _followersPreStream;
        public int FollowersPreStream
        {
            get => _followersPreStream;
            set
            {
                _followersPreStream = value;
                OnPropertyChanged(nameof(FollowersPreStream));
                FollowersDifference = CurrentFollowers - FollowersPreStream;
            }
        }

        private int _followersDifference;
        public int FollowersDifference
        {
            get => _followersDifference;
            set { _followersDifference = value; OnPropertyChanged(nameof(FollowersDifference)); }
        }

        private int _currentFollowers;
        public int CurrentFollowers
        {
            get => _currentFollowers;
            set
            {
                _currentFollowers = value;
                OnPropertyChanged(nameof(CurrentFollowers));
                FollowersDifference = CurrentFollowers - FollowersPreStream;
            }
        }

        private int _messagesSent;
        public int MessagesSent
        {
            get => _messagesSent;
            set { _messagesSent = value; OnPropertyChanged(nameof(MessagesSent)); }
        }

        private int _newSubscribers;
        public int NewSubscribers
        {
            get => _newSubscribers;
            set { _newSubscribers = value; OnPropertyChanged(nameof(NewSubscribers)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}