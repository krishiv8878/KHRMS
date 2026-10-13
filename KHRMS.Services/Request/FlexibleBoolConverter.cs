using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KHRMS.Services.Request
{
    public class FlexibleBoolConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long intVal))
                    {
                        return intVal > 0;
                    }
                    if (reader.TryGetDouble(out double dblVal))
                    {
                        return dblVal > 0;
                    }
                    return false;
                case JsonTokenType.String:
                    var str = reader.GetString();
                    if (string.IsNullOrWhiteSpace(str)) return null;
                    if (bool.TryParse(str, out bool bVal)) return bVal;
                    if (long.TryParse(str, out long numVal)) return numVal > 0;
                    return false;
                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteBooleanValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
