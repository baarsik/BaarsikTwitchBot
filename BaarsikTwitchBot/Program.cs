using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaarsikTwitchBot.Controllers;
using BaarsikTwitchBot.Domain;
using BaarsikTwitchBot.Extensions;
using BaarsikTwitchBot.Helpers;
using BaarsikTwitchBot.Interfaces;
using BaarsikTwitchBot.Models;
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

namespace BaarsikTwitchBot
{
    class Program
    {
        public static ServiceProvider ServiceProvider { get; set; }
        private static bool IsSuccessful { get; set; }

        public static readonly ILoggerFactory LoggerFactory
            = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Error);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            });

        public static void Main(string[] args)
        {
            Program.Log("Launching the bot");

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .Build();

            if (!VMProtect.SDK.IsProtected())
            {
                Program.Log("Program is not protected", LogLevel.Warning);
            }

            if (IsSuccessful)
            {
                ActivatorUtilities.CreateInstance<BotController>(host.Services);
                Program.Log("Successfully launched the bot");

                try
                {
                    host.Run();
                }
                catch (Exception e)
                {
                    Program.Log($"{e.Message} {e.StackTrace}", LogLevel.Error);
                    throw;
                }
                finally
                {
                    Program.Log("Closing the bot. Press any key to continue...");
                }
            }
            else
            {
                Program.Log("Failed to run the bot. Press any key to continue...", LogLevel.Error);
                Console.ReadKey();
            }
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private static void ConfigureServices(IServiceCollection services)
        {
            var config = GetConfig();

            if (!VMProtect.SDK.IsValidImageCRC())
            {
                Program.Log(VMProtect.SDK.DecryptString("Program files have been corrupted"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            if (string.IsNullOrEmpty(config.OAuth.ClientID) || string.IsNullOrEmpty(config.OAuth.ClientSecret))
            {
                Program.Log(VMProtect.SDK.DecryptString("ClientID and ClientSecret are required for the bot to work"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            if (string.IsNullOrEmpty(config.Channel.OAuth.Replace("oauth:", "")))
            {
                Program.Log(VMProtect.SDK.DecryptString("Twitch user OAuth is required for the bot to work"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            if (!string.Equals(config.Channel.Name, VMProtect.SDK.DecryptString(Constants.User.ChannelName), StringComparison.InvariantCultureIgnoreCase))
            {
                Program.Log(VMProtect.SDK.DecryptString($"Invalid channel name: '{config.Channel.Name}'"), LogLevel.Critical);
                IsSuccessful = false;
                return;
            }

            var autoRegisterTypes = new List<Type> { typeof(IChatHook), typeof(IAutoRegister) };
            var autoRegisterClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && x.GetInterfaces().Any(t => autoRegisterTypes.Contains(t)) && !x.IsAbstract)
                .ToList();

            var credentials = new ConnectionCredentials(config.Channel.Name, config.Channel.OAuth);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750
            };
            var webSocketClient = new WebSocketClient(clientOptions);
            var twitchClient = new TwitchClient(webSocketClient);
            twitchClient.Initialize(credentials, Constants.User.ChannelName);

            var twitchApi = new TwitchAPI
            {
                Settings =
                {
                    ClientId = config.OAuth.ClientID,
                    Secret = config.OAuth.ClientSecret
                }
            };

            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlServer(config.ConnectionString);
                options.UseLoggerFactory(LoggerFactory);
            });
            services.AddSingleton(config);
            services.AddSingleton(twitchClient);
            services.AddSingleton(twitchApi);
            services.AddSingleton<TwitchApiHelper>();
            services.AddSingleton<TwitchClientHelper>();
            services.AddScoped<DbHelper>();
            autoRegisterClasses.ForEach(x => services.AddScoped(x));

            services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

            ServiceProvider = services.BuildServiceProvider();

            IsSuccessful = true;
        }

        [Obfuscation(Feature = Constants.Obfuscation.Virtualization, Exclude = false)]
        private static JsonConfig GetConfig()
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
                Program.Log("Config file not found. Creating default file. Please update it manually", LogLevel.Critical);
            }
            return config;
        }

        #region Log

        public static void Log(string text)
        {
            Log(text, LogLevel.Information);
        }

        public static void Log(string text, LogLevel level)
        {
            var levelString = level == LogLevel.Information ? "INFO" : level.ToString().ToUpper();
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            Console.Write($"[{DateTime.Now:T}] [{levelString}] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);

            if (!Directory.Exists("./logs"))
            {
                Directory.CreateDirectory("./logs");
            }

            using var file = File.AppendText($"./logs/{DateTime.Now:yyyyMMdd}.log");
            file.WriteLine($"[{DateTime.Now:T}] [{levelString}] {text}");
            file.Close();
        }

        #endregion
    }
}
