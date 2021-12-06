using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using BaarsikTwitchBot.Annotations;
using BaarsikTwitchBot.Extensions;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Windows.ViewModels;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace BaarsikTwitchBot.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationWindow.xaml
    /// </summary>
    public partial class PreConfigurationWindow : INotifyPropertyChanged
    {
        private readonly JsonConfig _config;
        private readonly ILogger _logger;

        public PreConfigurationWindow(JsonConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;

            this.Loaded += OnLoaded;
            InitializeComponent();
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ClientID = _config.OAuth.ClientID;
            ViewModel.ClientSecret = _config.OAuth.ClientSecret;
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void NextButtonOnClick(object sender, RoutedEventArgs e)
        {
            _config.OAuth.ClientID = ViewModel.ClientID;
            _config.OAuth.ClientSecret = ViewModel.ClientSecret;
            _config.Save(_logger);
            MessageBox.Show(this, "Config has been saved. Please restart the application.", "StreamKiller", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        public PreConfigurationViewModel ViewModel { get; } = new();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
