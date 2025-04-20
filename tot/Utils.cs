using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using tot_lib;
using tot_lib.CommandLine;

namespace Tot;

public static class Utils
{
    public static bool AreShaIdentical(this List<PakedFile> files)
    {
        var sha = files[0].sha;
        foreach (var file in files)
            if (sha != file.sha)
                return false;
        return true;
    }

    public static ICommandBuilder<TCommand> AddModName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TCommand>(this IOptionCollection<TCommand> options, 
        Action<TCommand, string> setter) 
        where TCommand : class,ICommand<TCommand>
    {
        return options
            .Create<string>("--conan-mod", "Specify the mod name you want to perform the action on")
            .AddAlias("-m")
            .AddSetter((c, v) => setter(c,v ?? string.Empty))
            .BuildOption();
    }

    public static async Task EditWithCli(this Config config, string filePath) =>
        await config.EditWithCli(filePath, CancellationToken.None);
    public static async Task EditWithCli(this Config config, string filePath, CancellationToken token)
    {
        using (var fileOpener = new Process())
        {
            fileOpener.StartInfo.FileName = config.DefaultCliEditor;
            fileOpener.StartInfo.Arguments = $"\"{filePath}\"";
            fileOpener.StartInfo.UseShellExecute = false;
            fileOpener.Start();
            await fileOpener.WaitForExitAsync(token);
        }
    }
}