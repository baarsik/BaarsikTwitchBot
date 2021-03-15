using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
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

            var initWindow = ServiceProvider.GetService<InitWindow>();
            initWindow.Show();
        }

        private IServiceProvider ServiceProvider { get; set; }
        private ILogger Logger { get; set; }
        private JsonConfig Config { get; set; }
        private bool IsSuccessful { get; set; }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private void ConfigureServices(IServiceCollection services)
        {
            Logger = new Logger();

            Logger.Log($"Launching application v{Assembly.GetExecutingAssembly().GetName().Version}", LogLevel.Information);

            Config = GetConfig();

            if (!VMProtect.SDK.IsValidImageCRC())
            {
                Logger.Log(VMProtect.SDK.DecryptString("Program files have been corrupted"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            if (string.IsNullOrEmpty(Config.OAuth.ClientID) || string.IsNullOrEmpty(Config.OAuth.ClientSecret))
            {
                Logger.Log(VMProtect.SDK.DecryptString($"Please validate {nameof(JsonConfig.OAuth)} settings"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            if (string.IsNullOrEmpty(Config.Channel.OAuth.Replace("oauth:", "")))
            {
                Logger.Log(VMProtect.SDK.DecryptString($"Please validate {nameof(JsonConfig.Channel)} settings"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            if (!string.Equals(Config.Channel.Name, VMProtect.SDK.DecryptString(Constants.User.ChannelName), StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.Log(VMProtect.SDK.DecryptString($"Invalid channel name: '{Config.Channel.Name}'"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            if (string.IsNullOrEmpty(Config.BotUser.Name) || string.IsNullOrEmpty(Config.BotUser.OAuth.Replace("oauth:", "")))
            {
                Logger.Log(VMProtect.SDK.DecryptString($"Please validate {nameof(JsonConfig.BotUser)} settings"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            var autoRegisterTypes = new List<Type> { typeof(IChatHook), typeof(IAutoRegister) };
            var autoRegisterClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && x.GetInterfaces().Any(t => autoRegisterTypes.Contains(t)) && !x.IsAbstract)
                .ToList();

            var credentials = new ConnectionCredentials(Config.BotUser.Name, Config.BotUser.OAuth);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750
            };
            var webSocketClient = new WebSocketClient(clientOptions);
            var twitchClient = new TwitchClient(webSocketClient)
            {
                AutoReListenOnException = true
            };
            twitchClient.Initialize(credentials, Constants.User.ChannelName);

            var twitchApi = new TwitchAPI
            {
                Settings =
                {
                    ClientId = Config.OAuth.ClientID,
                    Secret = Config.OAuth.ClientSecret
                }
            };

            services.AddSingleton(Logger);
            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlServer(Config.ConnectionString);
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
            services.AddScoped<InitWindow>();
            services.AddScoped<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            IsSuccessful = true;
        }

        private JsonConfig GetConfig()
        {
            const string path = "./config/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileName = $"{path}config.json";
            var config = new JsonConfig();
            if (File.Exists(fileName))
            {
                config = JsonConvert.DeserializeObject<JsonConfig>(File.ReadAllText(fileName));
            }
            else
            {
                var fileContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(fileName, fileContent);
                Logger.Log("Config file not found. Creating default file. Please update it manually", LogLevel.Critical);
            }
            return config;
        }
    }
}
