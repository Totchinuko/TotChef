using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot.Services;

namespace Tot;

public partial class Stove(KitchenFiles kitchenFiles, ILogger<Stove> logger, Config config)
{
    private bool _verbose;

    public bool WasSuccess { get; private set; }
    public int Errors { get; private set; }
    public int Warnings { get; private set; }
    public ModStatus CurrentStatus { get; private set; } = new();

    public async Task StartCooking(CancellationToken cancellationToken, bool verbose = false, bool altOutput = false)
    {
        _verbose = verbose;

        if(File.Exists(kitchenFiles.ModStatus.FullName))
            CurrentStatus = await kitchenFiles.GetModStatus();
        CurrentStatus.WasSuccess = false;
        CurrentStatus.LastActionDate = DateTime.UtcNow;
        CurrentStatus.ErrorLogs.Clear();
        await kitchenFiles.SetModStatus(CurrentStatus);
            
        var process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.FileName = kitchenFiles.UnrealRunAutomation.FullName;
        process.StartInfo.WorkingDirectory = kitchenFiles.DevKit.FullName;
        process.StartInfo.Arguments = string.Join(" ",
            string.Join(" ", Constants.CookArgsFirstPass),
            Constants.CookProjectArg + "=\"" + kitchenFiles.UProject.FullName + "\"",
            Constants.CookScriptDirArg + "=\"" + kitchenFiles.ScriptDir.FullName + "\"",
            Constants.CookModArg + "=\"" + kitchenFiles.ModName + "\"",
            string.Join(" ", Constants.CookArgsSecondPass)
        );
        if (altOutput)
        {
            if (string.IsNullOrEmpty(config.AlternateOutputFolder) || !Directory.Exists(config.AlternateOutputFolder))
                throw new DirectoryNotFoundException("Alternate Output Direction \"" + config.AlternateOutputFolder +
                                                     "\" does not exists");
            process.StartInfo.Arguments +=
                " " + Constants.CookOutputDirArg + "=\"" + config.AlternateOutputFolder + "\"";
        }
        process.OutputDataReceived += OnOutputDataReceived;
        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(cancellationToken);
        await kitchenFiles.SetModStatus(CurrentStatus);
        if (!process.HasExited)
        {
            process.Kill();
            WasSuccess = false;
            return;
        }
        WasSuccess = process.ExitCode == 0;
        CurrentStatus.WasSuccess = WasSuccess;
        await kitchenFiles.SetModStatus(CurrentStatus);
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        var line = e.Data?.Trim().Replace("\n", "").Replace("\r", "") ?? "";
        if (string.IsNullOrEmpty(line)) return;

        // "^([0-9\\.\\-\\:\\[\\]\\s]+)LogInit:Display: (Failure|Success) - ([0-9,]+) error\\(s\\), ([0-9,]+) warning\\(s\\)$"
        var match = Regex.Match(line,
            "^LogInit:Display: (Failure|Success) - ([0-9,]+) error\\(s\\), ([0-9,]+) warning\\(s\\)$");
        if (match.Success)
        {
            Errors = int.Parse(match.Groups[2].Value);
            Warnings = int.Parse(match.Groups[3].Value);
        }

        ParseAndSend(line);
    }
    
    private void ParseAndSend(string output)
    {
        var lines = output.Trim().Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var matches = LogRegex().Match(line);
            if (!matches.Success)
            {
                if(!_verbose) continue;
                logger.LogInformation(line);
                continue;
            }
            
            var source = matches.Groups[1].Value.Trim();
            var level = ParseLogLevel(matches.Groups[2].Value);
            var content = matches.Groups[3].Value;

            if (level >= LogLevel.Error)
                CurrentStatus.ErrorLogs.Add($"[{source}][{level.ToString()}] {content}");
            if (level < LogLevel.Error && !_verbose) continue;
            
            using(logger.BeginScope(("DevKitSource", source)))
                logger.Log(level, content);
        }
    }
    
    //Fatal, Error, Warning, Display, Log, Verbose, VeryVerbose, All (=VeryVerbose)
    private LogLevel ParseLogLevel(string data)
    {
        data = data.ToLower().Trim();
        switch (data)
        {
            case "fatal":
                return LogLevel.Critical;
            case "error":
                return LogLevel.Error;
            case "warning":
                return LogLevel.Warning;
            case "display":
            case "log":
                return LogLevel.Information;
            default:
                return LogLevel.Information;
        }
    }
    
    //regexr /^\[([0-9\.\-\:]+)\]\[([0-9\s]+)\]([\w\s]+):(?:([\w\s]+):)?(.+)/
    [GeneratedRegex("^([\\w\\s]+):(?:([\\w\\s]+):)?(.+)")]
    private static partial Regex LogRegex();
}