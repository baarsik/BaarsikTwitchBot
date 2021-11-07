using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaarsikTwitchBot.Annotations;

namespace BaarsikTwitchBot.Windows.ViewModels
{
    public class ConfigurationViewModel : INotifyPropertyChanged
    {
        private string _clientId;
        public string ClientID
        {
            get => _clientId;
            set
            {
                _clientId = value;
                OnPropertyChanged(nameof(ClientID));
            }
        }

        private string _clientSecret;
        public string ClientSecret
        {
            get => _clientSecret;
            set
            {
                _clientSecret = value;
                OnPropertyChanged(nameof(ClientSecret));
            }
        }

        private string _channelOAuth;
        public string ChannelOAuth
        {
            get => _channelOAuth;
            set
            {
                _channelOAuth = value;
                OnPropertyChanged(nameof(ChannelOAuth));
            }
        }

        public string ChannelName { get; set; } = Constants.User.ChannelName;

        public string ChannelScope { get; set; } = Constants.Twitch.Scopes.Channel;

        private string _botUserOAuth;
        public string BotUserOAuth
        {
            get => _botUserOAuth;
            set
            {
                _botUserOAuth = value;
                OnPropertyChanged(nameof(BotUserOAuth));
            }
        }

        private string _botUserName;
        public string BotUserName
        {
            get => _botUserName;
            set
            {
                _botUserName = value;
                OnPropertyChanged(nameof(BotUserName));
            }
        }

        public string BotUserScope { get; set; } = Constants.Twitch.Scopes.BotUser;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}