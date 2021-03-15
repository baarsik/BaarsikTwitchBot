using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace BaarsikTwitchBot.Interfaces
{
    public interface ILogger : INotifyPropertyChanged
    {
        IList<string> History { get; }
        string HistoryText { get; }

        void Log(string text, LogLevel level);
    }
}