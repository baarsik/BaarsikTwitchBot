using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BaarsikTwitchBot.Annotations;
using BaarsikTwitchBot.Extensions;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Windows.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace BaarsikTwitchBot.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : INotifyPropertyChanged
    {
        private readonly JsonConfig _config;
        private readonly TwitchApiHelper _twitchApi;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public ConfigurationWindow(JsonConfig config, TwitchApiHelper twitchApi, ILogger logger, IServiceProvider serviceProvider)
        {
            _config = config;
            _twitchApi = twitchApi;
            _logger = logger;
            _serviceProvider = serviceProvider;

            this.Loaded += OnLoaded;
            InitializeComponent();
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ClientID = _config.OAuth.ClientID;
            ViewModel.BotUserOAuth = _config.BotUser.OAuth;
            ViewModel.BotUserName = _config.BotUser.Name;
            ViewModel.ChannelOAuth = _config.Channel.OAuth;
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private async void NextButtonOnClick(object sender, RoutedEventArgs e)
        {
            var isBotUserTokenValid = await _twitchApi.IsTokenValidAsync(ViewModel.BotUserOAuth, Constants.Twitch.Scopes.BotUser);
            var isChannelTokenValid = await _twitchApi.IsTokenValidAsync(ViewModel.ChannelOAuth, Constants.Twitch.Scopes.Channel);
            if (!isBotUserTokenValid || !isChannelTokenValid)
            {
                var botUserLine = $"{(isBotUserTokenValid ? string.Empty : "\nBot User token is invalid")}";
                var channelLine = $"{(isChannelTokenValid ? string.Empty : "\nChannel token is invalid")}";
                MessageBox.Show(this, $"Following error(s) occurred:\n{botUserLine}{channelLine}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _config.BotUser.Name = ViewModel.BotUserName;
            _config.BotUser.OAuth = ViewModel.BotUserOAuth;
            _config.Channel.OAuth = ViewModel.ChannelOAuth;
            _config.Save(_logger);
            MessageBox.Show(this, "Config has been saved. Please restart the application.", "StreamKiller", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        public ConfigurationViewModel ViewModel { get; } = new();

        private void OpenNextWindow<T>() where T : Window
        {
            this.Hide();
            var window = _serviceProvider.GetService<T>();
            window.Owner = this.Owner;
            window.Show();
            this.Close();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Grid grid })
            {
                var textBox = grid.Children.OfType<TextBox>().FirstOrDefault();
                try
                {
                    Clipboard.SetText(textBox.Text);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    MessageBox.Show(this, "Failed to copy", "StreamKiller", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
