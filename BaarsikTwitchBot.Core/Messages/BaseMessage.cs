using Newtonsoft.Json;

namespace BaarsikTwitchBot.Core.Messages
{
    public abstract class BaseMessage
    {
        protected virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public virtual byte[] ToByteArray()
        {
            return System.Text.Encoding.UTF8.GetBytes(ToJson());
        }
    }
}