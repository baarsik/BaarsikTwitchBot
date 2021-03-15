using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using BaarsikTwitchBot.Annotations;
using Microsoft.Extensions.Logging;

namespace BaarsikTwitchBot.Implementations
{
    public class Logger : Interfaces.ILogger
    {
        public IList<string> History { get; } = new List<string>();
        public string HistoryText { get; private set; }

        public void Log(string text, LogLevel level)
        {
            var levelString = level switch
            {
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                _ => level.ToString().ToUpper()
            };
            var line = $"[{DateTime.Now:T}] [{levelString}] {text}";
            
            History.Add(line);
            OnPropertyChanged(nameof(History));

            AppendLogFile(line);

            HistoryText = string.Join("\n", History);
            OnPropertyChanged(nameof(HistoryText));
        }

        private void AppendLogFile(string text)
        {
            if (!Directory.Exists("./logs"))
            {
                Directory.CreateDirectory("./logs");
            }
            using var file = File.AppendText($"./logs/{DateTime.Now:yyyyMMdd}.log");
            file.WriteLine(text);
            file.Close();
        }

        public override string ToString() => HistoryText;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}