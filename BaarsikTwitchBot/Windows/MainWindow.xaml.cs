using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using BaarsikTwitchBot.Annotations;
using BaarsikTwitchBot.Controllers;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations.AutoRegister;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Messaging.Sender;
using BaarsikTwitchBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace BaarsikTwitchBot.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly BotController _botController;
        private readonly TwitchApiHelper _apiHelper;
        private readonly TwitchClient _twitchClient;
        private readonly SongPlayerHandler _songPlayerHandler;
        private readonly JsonConfig _config;

        public MainWindow(BotController botController, ILogger logger, TwitchApiHelper apiHelper, TwitchClient twitchClient,
            SongPlayerHandler songPlayerHandler, JsonConfig config, IMessageSender messageSender)
        {
            _botController = botController;
            _apiHelper = apiHelper;
            _twitchClient = twitchClient;
            _songPlayerHandler = songPlayerHandler;
            _config = config;
            _messageSender = messageSender;
            Logger = logger;

            this.Loaded += OnLoaded;
            InitializeComponent();
        }

        public ILogger Logger { get; }
        public DashboardViewModel Dashboard { get; } = new();

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Title = $"StreamKiller: @{_config.Channel.Name}{(_config.Channel.Name == _config.BotUser.Name ? string.Empty : $" (bot: @{_config.BotUser.Name})")}";
            _botController.Initialize();

            Dashboard.CurrentFollowers = _apiHelper.BotUsers.Count(x => x.IsFollower);
            Dashboard.FollowersPreStream = _apiHelper.BotUsers.Count(x => x.IsFollower);
            _apiHelper.OnFollow += (_, user) => Dashboard.CurrentFollowers = _apiHelper.BotUsers.Count(x => x.IsFollower);
            _apiHelper.OnChannelSubscription += (_, args) => Dashboard.NewSubscribers++;

            _twitchClient.OnMessageReceived += OnTwitchChatMessageReceived;
            InitSongPlayer();
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void OnTwitchChatMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.StartsWith('!') || !string.IsNullOrEmpty(e.ChatMessage.CustomRewardId))
                return;

            Dashboard.MessagesSent++;
            Dashboard.AddChatter(e.ChatMessage.UserId);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
