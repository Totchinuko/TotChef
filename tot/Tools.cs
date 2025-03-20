using System.Text;

namespace Tot
{
    public static class Tools
    {
        public static bool AreShaIdentical(this List<PakedFile> files)
        {
            string sha = files[0].sha;
            foreach (PakedFile file in files)
                if (sha != file.sha) return false;
            return true;
        }

        public static DirectoryInfo GetProperCasedDirectoryInfo(this DirectoryInfo dirInfo)
        {
            if (!dirInfo.Exists)
            {
                return dirInfo;
            }

            if (dirInfo.Parent == null)
            {
                return dirInfo;
            }
            else
            {
                return dirInfo.Parent.GetProperCasedDirectoryInfo().GetDirectories(dirInfo.Name)[0];
            }
        }

        public static FileInfo GetProperCasedFileInfo(this FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                // Will not be able to match filesystem
                return fileInfo;
            }

            if (fileInfo.Directory == null)
            {
                return fileInfo;
            }

            return fileInfo.Directory.GetProperCasedDirectoryInfo().GetFiles(fileInfo.Name)[0];
        }

        public static string PosixFullName(this FileInfo fileInfo) => fileInfo.FullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        public static string PosixFullName(this DirectoryInfo dirInfo) => dirInfo.FullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        public static string PosixFullName(this string path) => path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        /// <summary>Reads a null-terminated string into a c# compatible string.</summary>
        /// <param name="input">Binary reader to pull the null-terminated string from.  Make sure it is correctly positioned in the stream before calling.</param>
        /// <returns>String of the same encoding as the input BinaryReader.</returns>
        public static string? ReadNullTerminatedString(this BinaryReader input)
        {
            StringBuilder sb = new StringBuilder(1000);
            char read = input.ReadChar();
            while (read != '\x00')
            {
                sb.Append(read);
                read = input.ReadChar();
            }
            string result = sb.ToString();
            return string.IsNullOrEmpty(result) ? null : result;
        }

        public static string RemoveExtension(this string path) => path[..^Path.GetExtension(path).Length];

        public static string RemoveRootFolder(this string path, string root)
        {
            string result = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Substring(root.Length);
            return result.StartsWith("\\") ? result.Substring(1) : result;
        }

        public static string RemoveRootFolder(this string path, FileInfo root)
        {
            return RemoveRootFolder(path, root?.DirectoryName ?? "");
        }

        public static string RemoveRootFolder(this string path, DirectoryInfo root)
        {
            return RemoveRootFolder(path, root.FullName);
        }

        public static void WriteColored(string text, ConsoleColor color, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = background;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void WriteColoredLine(string text, ConsoleColor color, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = background;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}