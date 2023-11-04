using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonPolymorphicConverter<TBaseType> : JsonConverter<TBaseType>
{
    private readonly string _discriminatorPropertyName;
    private readonly Dictionary<string, Type> _discriminatorToSubtype;
    private readonly Dictionary<Type, string> _subtypeToDiscriminator;

    public JsonPolymorphicConverter(
        string discriminatorPropertyName,
        Dictionary<string, Type> mappings
    )
    {
        _discriminatorPropertyName = discriminatorPropertyName;
        _discriminatorToSubtype = mappings;
        _subtypeToDiscriminator = _discriminatorToSubtype.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsAssignableTo(typeof(TBaseType)); // NOTE: By default (in the parent class's implementation), this only returns true if `typeToConvert` is *equal* to `T`.

    public override TBaseType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);

        string typeDiscriminator = jsonDoc.RootElement
            .GetProperty(_discriminatorPropertyName)
            .GetString()!;

        var type = _discriminatorToSubtype[typeDiscriminator];

        return (TBaseType?)jsonDoc.Deserialize(type, RemoveThisFromOptions(options));
    }

    public override void Write(Utf8JsonWriter writer, TBaseType value, JsonSerializerOptions options)
    {
        var type = value!.GetType();
        writer.WriteStartObject();

        writer.WriteString(_discriminatorPropertyName, _subtypeToDiscriminator[type]);

        using var jsonDoc = JsonSerializer.SerializeToDocument(value, type, RemoveThisFromOptions(options));
        foreach (var prop in jsonDoc.RootElement.EnumerateObject())
        {
            writer.WritePropertyName(prop.Name);
            prop.Value.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    private JsonSerializerOptions RemoveThisFromOptions(JsonSerializerOptions options)
    {
        JsonSerializerOptions newOptions = new(options);
        newOptions.Converters.Remove(this); // NOTE: We'll get an infinite loop if we don't do this
        return newOptions;
    }
}