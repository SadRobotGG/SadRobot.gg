using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SadRobot.Core.Json
{
    public class JsonConverterEpochDateTime : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var seconds = reader.GetInt64();
            if (seconds < 0) throw new FormatException("Invalid millenium epoch timestamp (less than 0)");
            return DateTimeExtensions.MilleniumEpoch.AddSeconds(seconds);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            var seconds = value.Value.ToEpoch(DateTimeExtensions.MilleniumEpoch);
            writer.WriteNumberValue(seconds);
        }
    }
}