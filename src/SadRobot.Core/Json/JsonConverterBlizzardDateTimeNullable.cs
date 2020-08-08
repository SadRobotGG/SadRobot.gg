using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SadRobot.Core.Json
{
    /// <summary>
    /// The Blizzard API returns date / time as a *nix epoch number, but with ms precision / padding
    /// </summary>
    public class JsonConverterBlizzardDateTimeNullable : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var seconds = reader.GetInt64();
            if (seconds < 0) throw new FormatException("Invalid Unix timestamp (less than 0)");
            return DateTime.UnixEpoch.AddSeconds(seconds / 1000);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            writer.WriteNumberValue(value.Value.ToEpoch(DateTime.UnixEpoch) * 1000);
        }
    }
}