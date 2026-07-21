using System.CommandLine;
using System.Text;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class TagsCommand(KitchenFiles files, GitHandler git, Config config, ILogger<TagsCommand> logger) : IInvokableCommand<TagsCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<TagsCommand>("tags", "Edit the tags of the selected mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            ModTags tags;
            try
            {
                tags = await files.GetModTags();
            }
            catch
            {
                tags = new ModTags();
            }
            var notSet = Constants.SteamTags.ToList();
            notSet.RemoveAll(x => tags.Tags.Contains(x));
            StringBuilder text = new();
            foreach (var tag in tags.Tags)
                text.AppendLine(tag);
            foreach (var tag in notSet)
                text.AppendLine($"-{tag}");
            
            var tmpFile = await files.CreateTemporaryTextFile(text.ToString());
            await config.EditWithCli(tmpFile, token);

            var result = (await File.ReadAllTextAsync(tmpFile, token)).Trim();
            File.Delete(tmpFile);
            
            tags.Tags.Clear();
            using (StringReader reader = new StringReader(result))
            {
                string? line;
                while ((line = await reader.ReadLineAsync(token)) != null)
                {
                    if(line.StartsWith("-")) continue;
                    tags.Tags.Add(line);
                }
            }

            logger.LogInformation($"Setting Tags: {string.Join(", ", tags.Tags)}");
            await files.SetModTags(tags);
            await git.CommitFile(files.ModFolder, files.ModTags, Constants.GitCommitTagsMessage);
            logger.LogInformation("Tag commited");
        }
        catch  (Exception ex)
        {
            logger.LogCritical(ex, "Failed to edit tags");
            return ex.GetErrorCode();
        }
        return 0;
    }
}
