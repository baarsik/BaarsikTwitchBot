﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using AdonisUI.Controls;
using BaarsikTwitchBot.Controllers;
using BaarsikTwitchBot.Domain;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Implementations;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
using BaarsikTwitchBot.Web;
using BaarsikTwitchBot.Windows;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;
using Application = System.Windows.Application;

namespace BaarsikTwitchBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mutex SingleRunMutex = new Mutex(true, "dc68d531-f931-44ec-806f-d9b16a83cb23");

        public App()
        {
            if (!SingleRunMutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("There is another bot instance running", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamKiller"));

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            var webHost = WebHost.CreateDefaultBuilder()
                .UseKestrel(x => x.ListenLocalhost(Config.WebServerLocalPort))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<WebStartup>()
                .Build()
                .RunAsync();

            if (!VMProtect.SDK.IsProtected())
            {
                Logger.Log("Program is not protected", LogLevel.Warning);
            }

            if (!IsSuccessful)
            {
                //MessageBox.Show($"Startup failed with following errors:\n\n{string.Join('\n', StartupErrorMessages)}", "StreamKiller - Startup Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                var configWindow = ServiceProvider.GetService<PreConfigurationWindow>();
                configWindow.Show();
                return;
            }
            
            var initWindow = ServiceProvider.GetService<InitWindow>();
            initWindow.Show();
        }

        private IList<string> StartupErrorMessages { get; set; } = new List<string>();
        private IServiceProvider ServiceProvider { get; set; }
        private ILogger Logger { get; set; }
        private JsonConfig Config { get; set; }
        private bool IsSuccessful => !StartupErrorMessages.Any();

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void ConfigureServices(IServiceCollection services)
        {
            Logger = new Logger();

            Logger.Log($"Launching application v{Assembly.GetExecutingAssembly().GetName().Version}", LogLevel.Information);

            Config = GetConfig();

            if (!VMProtect.SDK.IsValidImageCRC())
            {
                var errorMessage = VMProtect.SDK.DecryptString("Program files have been corrupted");
                StartupErrorMessages.Add(errorMessage);
                Logger.Log(errorMessage, LogLevel.Critical);
            }

            if (string.IsNullOrEmpty(Config.OAuth.ClientID) || string.IsNullOrEmpty(Config.OAuth.ClientSecret))
            {
                var errorMessage = VMProtect.SDK.DecryptString($"Please validate {nameof(JsonConfig.OAuth)} settings");
                StartupErrorMessages.Add(errorMessage);
                Logger.Log(errorMessage, LogLevel.Critical);
            }

            if (!string.Equals(Config.Channel.Name, VMProtect.SDK.DecryptString(Constants.User.ChannelName), StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(Config.Channel.Name))
            {
                var errorMessage = VMProtect.SDK.DecryptString($"Invalid channel name: '{Config.Channel.Name}'");
                StartupErrorMessages.Add(errorMessage);
                Logger.Log(errorMessage, LogLevel.Critical);
            }

            var autoRegisterTypes = new List<Type> { typeof(IChatHook), typeof(IAutoRegister) };
            var autoRegisterClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && x.GetInterfaces().Any(t => autoRegisterTypes.Contains(t)) && !x.IsAbstract)
                .ToList();

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750
            };
            var webSocketClient = new WebSocketClient(clientOptions);
            var twitchClient = new TwitchClient(webSocketClient)
            {
                AutoReListenOnException = true
            };

            if (IsSuccessful)
            {
                var credentials = new ConnectionCredentials(Config.BotUser.Name, Config.BotUser.OAuth);
                twitchClient.Initialize(credentials, Constants.User.ChannelName);
            }

            var twitchApi = new TwitchAPI
            {
                Settings =
                {
                    ClientId = Config.OAuth.ClientID,
                    Secret = Config.OAuth.ClientSecret
                }
            };

            services.AddSingleton(Logger);
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamKiller", "data.db");
            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlite($"Filename={path}");
            });
            services.AddSingleton(Config);
            services.AddSingleton(twitchClient);
            services.AddSingleton(twitchApi);
            services.AddSingleton<TwitchApiHelper>();
            services.AddSingleton<TwitchClientHelper>();
            services.AddScoped<DbHelper>();
            autoRegisterClasses.ForEach(x => services.AddScoped(x));

            services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

            services.AddSingleton<BotController>();

            #region WPF Windows (Scoped)
            services.AddScoped<InitWindow>();
            services.AddScoped<MainWindow>();
            services.AddScoped<PreConfigurationWindow>();
            services.AddScoped<ConfigurationWindow>();
            #endregion

            ServiceProvider = services.BuildServiceProvider();
        }

        private JsonConfig GetConfig()
        {
            #if DEBUG
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamKiller", "config_debug.json");
            #else
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamKiller", "config.json");
            #endif
            
            var config = new JsonConfig();
            if (File.Exists(filePath))
            {
                config = JsonConvert.DeserializeObject<JsonConfig>(File.ReadAllText(filePath));
            }
            else
            {
                var fileContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, fileContent);
                Logger.Log("Config file not found. Creating default file. Please update it manually", LogLevel.Critical);
            }
            return config;
        }
    }
}
