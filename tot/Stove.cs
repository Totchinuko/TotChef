using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot.Services;

namespace Tot;

public partial class Stove
{
    private readonly ILogger<Stove> _logger;
    private readonly Process _process;
    private bool _verbose;

    public Stove(KitchenFiles kitchenFiles, ILogger<Stove> logger)
    {
        _logger = logger;
        _process = new Process();
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.StartInfo.FileName = kitchenFiles.Ue4Cmd.FullName;
        _process.StartInfo.WorkingDirectory = kitchenFiles.DevKit.FullName;
        _process.StartInfo.EnvironmentVariables["=C:"] = "C:\\";
        _process.StartInfo.EnvironmentVariables[Constants.GraniteSdkEnvKey] = kitchenFiles.GraniteSdkDir.FullName;
        _process.StartInfo.Arguments = string.Join(" ",
            "\"" + kitchenFiles.UProject.FullName + "\"",
            string.Join(" ", Constants.CookArgs),
            Constants.CookLogArg + "=" + kitchenFiles.CookLogFile.FullName
        );
        _process.OutputDataReceived += OnOutputDataReceived;
    }

    public bool WasSuccess { get; private set; }
    public int Errors { get; private set; }
    public int Warnings { get; private set; }

    public async Task StartCooking(CancellationToken cancellationToken, bool verbose = false)
    {
        _verbose = verbose;
        _process.Start();
        _process.BeginOutputReadLine();
        await _process.WaitForExitAsync(cancellationToken);
        if (!_process.HasExited)
        {
            _process.Kill();
            WasSuccess = false;
            return;
        }
        WasSuccess = _process.ExitCode == 0;
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        var line = e.Data?.Trim().Replace("\n", "").Replace("\r", "") ?? "";
        if (string.IsNullOrEmpty(line)) return;

        var match = Regex.Match(line,
            "^([0-9\\.\\-\\:\\[\\]\\s]+)LogInit:Display: (Failure|Success) - ([0-9,]+) error\\(s\\), ([0-9,]+) warning\\(s\\)$");
        if (match.Success)
        {
            Errors = int.Parse(match.Groups[3].Value);
            Warnings = int.Parse(match.Groups[4].Value);
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
                _logger.LogInformation(line);
                continue;
            }
            
            //var date = ParseDate(matches.Groups[1].Value);
            var source = matches.Groups[3].Value.Trim();
            var level = ParseLogLevel(matches.Groups[4].Value);
            var content = matches.Groups[5].Value;

            if (level < LogLevel.Error && !_verbose) continue;
            
            using(_logger.BeginScope(("DevKitSource", source)))
                _logger.Log(level, content);
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
    [GeneratedRegex("^\\[([0-9\\.\\-\\:]+)\\]\\[([0-9\\s]+)\\]([\\w\\s]+):(?:([\\w\\s]+):)?(.+)")]
    private static partial Regex LogRegex();
}