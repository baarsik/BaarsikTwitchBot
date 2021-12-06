using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaarsikTwitchBot.Annotations;

namespace BaarsikTwitchBot.Windows.ViewModels
{
    public class PreConfigurationViewModel : INotifyPropertyChanged
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

        public string BotUserScope { get; set; } = Constants.Twitch.Scopes.BotUser;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}