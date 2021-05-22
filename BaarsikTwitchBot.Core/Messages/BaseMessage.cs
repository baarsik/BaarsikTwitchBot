using System.Text;
using Newtonsoft.Json;

namespace BaarsikTwitchBot.Core.Messages
{
    public abstract class BaseMessage
    {
        protected virtual string ToJson()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.SerializeObject(this,settings);
        }

        public override string ToString()
        {
            return ToJson();
        }

        public virtual byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToJson());
        }
    }
}