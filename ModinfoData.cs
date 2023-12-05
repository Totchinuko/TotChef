using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot
{
    internal class ModinfoData
    {
        public string Author { get; set; } = string.Empty;

        public bool BRequiresLoadOnStartup { get; set; } = false;

        public string ChangeNote { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int FileSize { get; set; } = -2;

        public string FolderName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int RevisionNumber { get; set; } = 0;

        public int SnapshotId { get; set; } = 0;

        public string SteamPublishedFileId { get; set; } = string.Empty;

        public string SteamTestLivePublishedFileId { get; set; } = string.Empty;

        public int SteamVisibility { get; set; } = 0;

        public int VersionBuild { get; set; }

        public int VersionMajor { get; set; } = 0;

        public int VersionMinor { get; set; } = 0;
    }
}