using System.Text.Json.Serialization;

namespace Tot;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
[JsonSerializable(typeof(ModinfoData))]
public partial class ModinfoDataJsonContext : JsonSerializerContext
{
}

public class ModinfoData
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    [JsonPropertyName("changeNote")] public string ChangeNote { get; set; } = string.Empty;

    [JsonPropertyName("author")] public string Author { get; set; } = string.Empty;

    [JsonPropertyName("authorUrl")] public string AuthorUrl { get; set; } = string.Empty;

    [JsonPropertyName("versionMajor")] public int VersionMajor { get; set; } = 0;

    [JsonPropertyName("versionMinor")] public int VersionMinor { get; set; } = 0;

    [JsonPropertyName("versionBuild")] public int VersionBuild { get; set; }

    [JsonPropertyName("bRequiresLoadOnStartup")]
    public bool BRequiresLoadOnStartup { get; set; } = false;

    [JsonPropertyName("steamPublishedFileId")]
    public string SteamPublishedFileId { get; set; } = string.Empty;

    [JsonPropertyName("steamTestLivePublishedFileId")]
    public string SteamTestLivePublishedFileId { get; set; } = string.Empty;

    [JsonPropertyName("steamVisibility")] public int SteamVisibility { get; set; } = 0;

    [JsonPropertyName("folderName")] public string FolderName { get; set; } = string.Empty;

    [JsonPropertyName("revisionNumber")] public int RevisionNumber { get; set; } = 0;

    [JsonPropertyName("snapshotId")] public int SnapshotId { get; set; } = 0;

    [JsonPropertyName("fileSize")] public int FileSize { get; set; } = -2;
}