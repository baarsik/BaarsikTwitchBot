using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using BaarsikTwitchBot.Annotations;
using BaarsikTwitchBot.Controllers;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Models;
using Microsoft.Extensions.DependencyInjection;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace BaarsikTwitchBot.Windows
{
    /// <summary>
    /// Логика взаимодействия для InitWindow.xaml
    /// </summary>
    public partial class InitWindow : INotifyPropertyChanged
    {
        private readonly BotController _botController;
        private readonly IServiceProvider _serviceProvider;
        private readonly SongPlayerHandler _songPlayerHandler;

        public InitWindow(BotController botController, IServiceProvider serviceProvider, SongPlayerHandler songPlayerHandler)
        {
            _botController = botController;
            _serviceProvider = serviceProvider;
            _songPlayerHandler = songPlayerHandler;
            InitializeComponent();
        }

        private async void OnContentRendered(object sender, EventArgs e)
        {
            var validationResult =
                await _botController.ValidateChannelCredentials() &&
                await _botController.ValidateBotUserCredentials();
            
            if (validationResult)
            {
                _botController.Initialize();
                _songPlayerHandler.Initialize();
                OpenNextWindow<MainWindow>();
            }
            else
            {
                this.Hide();
                switch (_botController.InitializationStatus)
                {
                    case BotInitializationStatus.ChannelCredentialsValidated - 1:
                        MessageBox.Show("Channel credentials invalid", "StreamKiller - Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case BotInitializationStatus.BotUserCredentialsValidated - 1:
                        MessageBox.Show("Bot user credentials invalid", "StreamKiller - Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
                this.Close();
            }
        }

        private string _loadingText = "Loading";
        public string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
                OnPropertyChanged(nameof(LoadingText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenNextWindow<T>() where T : Window
        {
            this.Hide();
            var window = _serviceProvider.GetService<T>();
            window.Owner = this.Owner;
            window.Show();
            this.Close();
        }
    }
}
