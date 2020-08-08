using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SadRobot.Core.Json
{
    public class JsonConverterUnixDateTime : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var seconds = reader.GetInt64();
            if (seconds < 0) throw new FormatException("Invalid Unix timestamp (less than 0)");
            return DateTime.UnixEpoch.AddSeconds(seconds);
        }
        
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.ToEpoch(DateTime.UnixEpoch));
        }
    }
}