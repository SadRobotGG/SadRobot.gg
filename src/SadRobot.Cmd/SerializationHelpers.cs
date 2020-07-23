using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SadRobot.Cmd
{
    public static class SerializationHelpers
    {
        public static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
    }
}