using System.Text.Json.Serialization;

namespace Tot;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
[JsonSerializable(typeof(ModStatus))]
public partial class ModStatusJsonContext : JsonSerializerContext
{
}

public class ModStatus
{
    [JsonPropertyName("lastCookAttempt")] public DateTime LastActionDate { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("errorLogs")] public List<string> ErrorLogs { get; set; } = new();
    [JsonPropertyName("wasSuccess")] public bool WasSuccess { get; set; } = false;
    [JsonPropertyName("wasUploaded")] public bool WasUploaded { get; set; } = false;
}