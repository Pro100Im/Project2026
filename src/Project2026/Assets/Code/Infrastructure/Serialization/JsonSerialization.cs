using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;

namespace Code.Infrastructure.Serialization
{
    public static class JsonSerialization
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Converters = { new StringEnumConverter() }
        };

        public static string ToJson(this object self) =>
          JsonConvert.SerializeObject(self, Settings);

        public static T FromJson<T>(this string json) =>
          JsonConvert.DeserializeObject<T>(json, Settings);

        static JsonSerialization() =>
          AotHelper.Ensure(() => new ReferenceConverter(typeof(Dummy)));

        public class Dummy { }
    }
}