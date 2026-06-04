using System.Text.Json.Serialization;

namespace Tot;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
[JsonSerializable(typeof(ModTags))]
public partial class ModTagsJsonContext : JsonSerializerContext
{
}

public class ModTags
{
    [JsonPropertyName("Tags")] public List<string> Tags { get; set; } = new();
}