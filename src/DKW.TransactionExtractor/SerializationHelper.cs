using System.Text.Json;
using System.Text.Json.Serialization;

namespace DKW.TransactionExtractor;

public static class SerializationHelper
{
    public static readonly JsonSerializerOptions JSO = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
