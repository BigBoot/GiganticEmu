using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CSharp.RuntimeBinder;

public class DynamicJsonConverter : JsonConverter<dynamic>
{
    public override dynamic? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.TryGetInt64(out long l) ? l : reader.GetDouble(),
            JsonTokenType.String => reader.TryGetDateTime(out DateTime datetime) ? datetime.ToString() : reader.GetString(),
            JsonTokenType.StartObject => ReadObject(JsonDocument.ParseValue(ref reader).RootElement),
            JsonTokenType.StartArray => ReadList(JsonDocument.ParseValue(ref reader).RootElement),
            // Use JsonElement as fallback.
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
        };

    private object ReadObject(JsonElement jsonElement)
    {
        IDictionary<string, object> expandoObject = new ExpandoObject()!;
        foreach (var obj in jsonElement.EnumerateObject())
        {
            var k = obj.Name;
            if (ReadValue(obj.Value) is object value)
            {
                expandoObject[k] = value;
            }
        }
        return expandoObject;
    }

    private object? ReadValue(JsonElement jsonElement)
        =>
         jsonElement.ValueKind switch
         {
             JsonValueKind.Object => ReadObject(jsonElement),
             JsonValueKind.Array => ReadList(jsonElement),
             JsonValueKind.String => jsonElement.GetString(),
             JsonValueKind.Number => jsonElement.TryGetInt64(out long l) ? l : 0,
             JsonValueKind.True => true,
             JsonValueKind.False => false,
             JsonValueKind.Undefined => null,
             JsonValueKind.Null => null,
             _ => throw new ArgumentOutOfRangeException()
         };

    private object? ReadList(JsonElement jsonElement)
    {
        var list = new List<object>();
        jsonElement.EnumerateArray()
            .Select(e => ReadValue(e))
            .OfType<object>()
            .ToList()
            .ForEach(o => list.Add(o));

        return list.Count == 0 ? null : list;
    }

    public override void Write(Utf8JsonWriter writer,
        object value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    public static T? GetProperty<T>(Func<T> getter)
    {
        try
        {
            return getter();
        }
        catch (RuntimeBinderException)
        {
            return default(T);
        }
    }
}