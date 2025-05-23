﻿namespace Tot;

public class PakListing
{
    public List<PakedFile> pakedFiles;
    public string pakName;
    public string rapport;

    public PakListing(string output, string pakName)
    {
        List<string> lines = new List<string>(output.Replace("LogPakFile:Display: ", "")
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
        this.pakName = pakName;

        rapport = lines.Last();
        lines.RemoveAt(lines.Count - 1);
        lines.Sort();

        pakedFiles = new List<PakedFile>();
        foreach (var line in lines)
        {
            var text = line;
            if (!text.StartsWith("\"")) continue;
            List<string> stack = new List<string>(text.Split(" ", StringSplitOptions.RemoveEmptyEntries));
            pakedFiles.Add(new PakedFile
            {
                pakName = pakName,
                path = stack[0].Substring(1)[..^1],
                sha = stack[stack.FindIndex(x => x == "sha1:") + 1][..^1],
                size = long.Parse(stack[stack.FindIndex(x => x == "size:") + 1])
            });
        }
    }
}