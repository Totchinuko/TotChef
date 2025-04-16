using System.Text.Json.Serialization;

namespace Tot;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
[JsonSerializable(typeof(PatchNote))]
public partial class PatchNoteJsonContext : JsonSerializerContext
{
}

public class PatchNote
{
    public List<string> Additions { get; set; } = [];
    public List<string> Improvements { get; set; } = [];
    public List<string> Fixes { get; set; } = [];
}