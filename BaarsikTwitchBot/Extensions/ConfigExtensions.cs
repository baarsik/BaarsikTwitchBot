using System;
using System.IO;
using BaarsikTwitchBot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ILogger = BaarsikTwitchBot.Interfaces.ILogger;

namespace BaarsikTwitchBot.Extensions
{
    public static class ConfigExtensions
    {
        public static void Save(this JsonConfig config, ILogger logger)
        {
#if DEBUG
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamKiller", "config_debug.json");
#else
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamKiller", "config.json");
#endif

            try
            {
                var fileContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, fileContent);
                logger.Log("Config has been updated", LogLevel.Information);
            }
            catch (Exception e)
            {
                logger.Log($"Exception occurred: {e.Message}", LogLevel.Error);
            }
        }
    }
}