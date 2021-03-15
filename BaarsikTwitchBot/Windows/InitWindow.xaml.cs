using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using BaarsikTwitchBot.Annotations;
using BaarsikTwitchBot.Controllers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using Microsoft.Extensions.DependencyInjection;

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

        private void OnContentRendered(object sender, EventArgs e)
        {
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() =>
                {
                    _botController.Initialize();
                    _songPlayerHandler.Initialize();
                })
                .ContinueWith(task =>
                {
                    OpenNextWindow<MainWindow>();
                }, uiScheduler);
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
