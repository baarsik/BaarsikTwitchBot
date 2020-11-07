using System;

namespace BaarsikTwitchBot.Interfaces
{
    public interface IAutoRegister
    {
        bool IsInitialized { get; }

        void Initialize();
    }
}