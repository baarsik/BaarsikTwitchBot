using BaarsikTwitchBot.Core.Messages;

namespace BaarsikTwitchBot.Messaging.Sender
{
    public interface IMessageSender
    {
        void SendMessage(BaseMessage message);
    }
}