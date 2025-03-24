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
}