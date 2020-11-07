using System.Collections.Generic;
using BaarsikTwitchBot.Models;
using TwitchLib.Client.Models;

namespace BaarsikTwitchBot.Interfaces
{
    public interface IChatHook
    {
        bool IsEnabled { get; }

        ChatHookAccessType Access { get; }

        IList<string> CommandNames { get; }

        void OnMessageReceived(ChatMessage chatMessage, IList<string> parameters);
    }
}