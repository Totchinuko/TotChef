using System.CommandLine;
using System.Xml;
using Microsoft.Extensions.Logging;
using Pastel;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class UploadCommand(KitchenFiles files, SteamWorks steamworks, ILogger<UploadCommand> logger) : IInvokableCommand<UploadCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<UploadCommand>("upload", "Upload the target mod to the workshop - Steam must be running")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .Options.Create<bool>("--skip-content", "Skip the upload of the image and pak file").AddAlias("-sc")
        .SetSetter((c,v) => c.SkipContent = v).BuildOption()
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;
    public bool SkipContent { get; set; }
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            var status = await files.GetModStatus();
            await steamworks.UploadMod(token, SkipContent);
            status.WasUploaded = true;
            status.LastActionDate = DateTime.UtcNow;
            await files.SetModStatus(status);
        }
        catch  (Exception ex)
        {
            logger.LogCritical(ex, "Failed to upload mod");
            return ex.GetErrorCode();
        }
        
        return 0;
    }
}
