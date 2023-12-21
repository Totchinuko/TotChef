using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tot
{
    internal class ModinfoData
    {
        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("bRequiresLoadOnStartup")]
        public bool BRequiresLoadOnStartup { get; set; } = false;

        [JsonPropertyName("changeNote")]
        public string ChangeNote { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("fileSize")]
        public int FileSize { get; set; } = -2;

        [JsonPropertyName("folderName")]
        public string FolderName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("revisionNumber")]
        public int RevisionNumber { get; set; } = 0;

        [JsonPropertyName("snapshotId")]
        public int SnapshotId { get; set; } = 0;

        [JsonPropertyName("steamPublishedFileId")]
        public string SteamPublishedFileId { get; set; } = string.Empty;

        [JsonPropertyName("steamTestLivePublishedFileId")]
        public string SteamTestLivePublishedFileId { get; set; } = string.Empty;

        [JsonPropertyName("steamVisibility")]
        public int SteamVisibility { get; set; } = 0;

        [JsonPropertyName("versionBuild")]
        public int VersionBuild { get; set; }

        [JsonPropertyName("versionMajor")]
        public int VersionMajor { get; set; } = 0;

        [JsonPropertyName("versionMinor")]
        public int VersionMinor { get; set; } = 0;
    }
}