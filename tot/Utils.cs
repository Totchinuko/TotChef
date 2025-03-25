using System.CommandLine;
using tot_lib;

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

    public static Option GetModNameOption(Action<string> setter)
    {
        var option = new TotOption<string>("--conan-mod",
            "Specify the mod name you want to perform the action on");
        option.AddAlias("-m");
        option.SetDefaultValue(string.Empty);
        option.AddSetter(x => setter(x ?? string.Empty));
        return option;
    }
}