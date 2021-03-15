using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;

namespace BaarsikTwitchBot.Controllers
{
    public class BotController
    {
        private readonly TwitchClient _twitchClient;
        private readonly TwitchApiHelper _apiHelper;
        private readonly IList<IChatHook> _chatHooks = new List<IChatHook>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public BotController(TwitchClient twitchClient, TwitchApiHelper apiHelper, IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _twitchClient = twitchClient;
            _apiHelper = apiHelper;
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            InitTwitchClient();
            InitChatHooks();
            InitAutoRegisteredClasses();

            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (!e.ChatMessage.Message.StartsWith('!')) return;

            var command = e.ChatMessage.Message.Split(' ').FirstOrDefault()?.Substring(1);
            var parameters = e.ChatMessage.Message.Split(' ').Skip(1).ToList();

            var botUser = _apiHelper.BotUsers.FirstOrDefault(x => x.UserId == e.ChatMessage.UserId);

            if (botUser?.IsBanned == true)
                return;

            foreach (var chatHook in _chatHooks.Where(x => x.IsEnabled))
            {
                if (!chatHook.CommandNames.Any(name => string.Equals(name, command, StringComparison.CurrentCultureIgnoreCase)))
                    continue;

                if (botUser == null)
                {
                    _twitchClient.SendMessage(e.ChatMessage.Channel, $"@{e.ChatMessage.Username}, только фолловеры могут пользоваться ботом");
                    break;
                }

                var hasAccess = chatHook.Access switch
                {
                    ChatHookAccessType.Everyone => true,
                    ChatHookAccessType.VIP => e.ChatMessage.IsVip || e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster,
                    ChatHookAccessType.Moderators => e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster,
                    ChatHookAccessType.Broadcaster => e.ChatMessage.IsBroadcaster,
                    _ => throw new ArgumentException()
                };
                if (!hasAccess)
                    continue;

                chatHook.OnMessageReceived(e.ChatMessage, parameters);
            }
        }

        public void Hook<T>()
        {
            RegisterHook(typeof(T));
        }

        public void Hook(List<Type> types)
        {
            types.ForEach(RegisterHook);
        }

        private void RegisterHook(Type type)
        {
            var instance = _serviceProvider.GetService(type);
            switch (instance)
            {
                case IChatHook hook:
                    _chatHooks.Add(hook);
                    break;
                default:
                    throw new InvalidConstraintException($"Invalid hook: {type.Name}");
            }
        }

        #region Initialization

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void InitTwitchClient()
        {
            _twitchClient.OnConnected += (sender, args) => _logger.Log($"Connected to '{Constants.User.ChannelName}' chat as '{args.BotUsername}'", LogLevel.Information);
            _twitchClient.OnConnectionError += (sender, args) => _logger.Log($"Failed to connect: {args.Error.Message}", LogLevel.Error);
            _twitchClient.OnDisconnected += (sender, args) =>
            {
                _logger.Log("Bot has been disconnected from IRC. Attempting reconnect", LogLevel.Warning);
                while (!_twitchClient.IsConnected)
                {
                    Thread.Sleep(1000);
                    _twitchClient.Connect();
                }
            };
            _twitchClient.OnMessageReceived += OnMessageReceived;

            _twitchClient.Connect();
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void InitChatHooks()
        {
            var chatHooks = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && x.GetInterfaces().Contains(typeof(IChatHook)) && !x.IsAbstract)
                .ToList();
            this.Hook(chatHooks);
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void InitAutoRegisteredClasses()
        {
            var autoRegisteredClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && x.GetInterfaces().Contains(typeof(IAutoRegister)) && !x.IsAbstract)
                .Select(type => _serviceProvider.GetService(type) as IAutoRegister)
                .ToList();

            autoRegisteredClasses.ForEach(x => x.Initialize());
        }

        #endregion
    }
}