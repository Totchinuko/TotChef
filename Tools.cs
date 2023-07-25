using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Tot
{
    public static class Tools
    {
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

        public static void WriteColoredLine(string text, ConsoleColor color, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = background;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteColored(string text, ConsoleColor color, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = background;
            Console.Write(text);
            Console.ResetColor();
        }

        public static bool AreShaIdentical(this List<PakedFile> files)
        {
            string sha = files[0].sha;
            foreach (PakedFile file in files)
                if (sha != file.sha) return false;
            return true;
        }
    }
}
