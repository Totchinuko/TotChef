using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using tot_lib;
using tot.Services;

namespace Tot;

public class Stove
{
    private readonly IColoredConsole _console;
    private readonly Process _process;
    private bool _verbose;

    public Stove(KitchenFiles kitchenFiles, IColoredConsole console)
    {
        _console = console;
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

        match = Regex.Match(line, "^([0-9\\.\\-\\:\\[\\]\\s]+)LogInit:Display: LogBlueprint:Error:");
        if (match.Success)
            _console.Error.WriteLine(line);
        else if (_verbose)
            _console.WriteLine(line);
    }
}