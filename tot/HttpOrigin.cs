using System.Text.Json.Serialization;

namespace Tot;

public class HttpOrigin
{
    [JsonPropertyName("origin")] public string Origin { get; set; } = string.Empty;
    [JsonPropertyName("reason")] public string Reason { get; set; } = string.Empty;
}