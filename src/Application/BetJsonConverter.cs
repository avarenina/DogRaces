using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Bets;

namespace Application;

public static class JsonConfig
{
    public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        Converters = { new BetJsonConverter() }
    };
}
public class BetJsonConverter : JsonConverter<Bet>
{

    public override Bet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("Type", out JsonElement betTypeProp))
        {
            throw new JsonException("Missing 'betType' property");
        }


        if (!betTypeProp.TryGetInt32(out int betTypeValue))
        {
            throw new JsonException("'betType' is not a valid integer");
        }

        if (!Enum.IsDefined(typeof(BetType), betTypeValue))
        {
            throw new JsonException($"Invalid BetType value: {betTypeValue}");
        }

        var betType = (BetType)betTypeValue;

        Bet result = betType switch
        {
            BetType.Winner => JsonSerializer.Deserialize<WinnerBet>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown BetType: {betType}")
        };

        return result;
    }

    public override void Write(Utf8JsonWriter writer, Bet value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}
