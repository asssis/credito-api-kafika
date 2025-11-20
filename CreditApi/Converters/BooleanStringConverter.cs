using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CreditApi.Converters
{
    public class BooleanStringConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString() ?? string.Empty;
                s = s.Trim();

                if (string.Equals(s, "sim", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "s", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "true", StringComparison.OrdinalIgnoreCase) ||
                    s == "1")
                    return true;

                if (string.Equals(s, "não", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "nao", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "n", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "no", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "false", StringComparison.OrdinalIgnoreCase) ||
                    s == "0")
                    return false;

                if (bool.TryParse(s, out var parsed)) return parsed;

                throw new JsonException($"Cannot convert value '{s}' to boolean.");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out var i))
                    return i != 0;
            }

            throw new JsonException($"Unhandled token type {reader.TokenType} when parsing boolean.");
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
