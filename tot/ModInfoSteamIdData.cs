using System.Text.Json.Serialization;

namespace Tot;

public class ModInfoSteamIdData
{
    [JsonPropertyName("mainClient")]
    public string MainClient { get; set; } = string.Empty;
}