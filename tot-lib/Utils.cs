using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using tot_lib.Git;

namespace tot_lib;

public static class Utils
{
    public static DirectoryInfo GetProperCasedDirectoryInfo(this DirectoryInfo dirInfo)
    {
        if (!dirInfo.Exists) return dirInfo;

        if (dirInfo.Parent == null) return dirInfo;

        return dirInfo.Parent.GetProperCasedDirectoryInfo().GetDirectories(dirInfo.Name)[0];
    }

    public static FileInfo GetProperCasedFileInfo(this FileInfo fileInfo)
    {
        if (!fileInfo.Exists)
            // Will not be able to match filesystem
            return fileInfo;

        if (fileInfo.Directory == null) return fileInfo;

        return fileInfo.Directory.GetProperCasedDirectoryInfo().GetFiles(fileInfo.Name)[0];
    }

    public static string PosixFullName(this FileInfo fileInfo)
    {
        return fileInfo.FullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static string PosixFullName(this DirectoryInfo dirInfo)
    {
        return dirInfo.FullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static string PosixFullName(this string path)
    {
        return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>Reads a null-terminated string into a c# compatible string.</summary>
    /// <param name="input">
    ///     Binary reader to pull the null-terminated string from.  Make sure it is correctly positioned in the
    ///     stream before calling.
    /// </param>
    /// <returns>String of the same encoding as the input BinaryReader.</returns>
    public static string? ReadNullTerminatedString(this BinaryReader input)
    {
        var sb = new StringBuilder(1000);
        var read = input.ReadChar();
        while (read != '\x00')
        {
            sb.Append(read);
            read = input.ReadChar();
        }

        var result = sb.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }

    public static string RemoveExtension(this string path)
    {
        return path[..^Path.GetExtension(path).Length];
    }

    public static string RemoveRootFolder(this string path, string root)
    {
        var result = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Substring(root.Length);
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

    public static void CreateTot<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
        (this RootCommand root, Action<IServiceCollection> serviceConfiguration) where T : class, ITotCommand
    {
        root.AddCommand(TotCommand.Create<T>(serviceConfiguration));
    }
}