using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tot
{
    internal class Stove
    {
        private bool verbose;
        Process process;

        public bool wasSuccess { get; private set; }
        public int errors { get; private set; }
        public int warnings { get; private set; }

        public Stove(KitchenClerk clerk, bool verbose = false) 
        {
            this.verbose = verbose;
            process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = clerk.UE4CMD.FullName;
            process.StartInfo.Arguments = string.Join(" ",
                    "\"" + clerk.UProject.FullName + "\"",
            string.Join(" ", clerk.CookArgs),
                    KitchenClerk.CookLogArg + "=" + clerk.CookLogFile.FullName
                );
            process.OutputDataReceived += OnOutputDataReceived;
        }

        public void StartCooking()
        {
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            wasSuccess = process.ExitCode == 0;
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data?.Trim().Replace("\n", "").Replace("\r", "") ?? "";
            if (string.IsNullOrEmpty(line)) return;

            Match match = Regex.Match(line, "^([0-9\\.\\-\\:\\[\\]\\s]+)LogInit:Display: (Failure|Success) - ([0-9,]+) error\\(s\\), ([0-9,]+) warning\\(s\\)$");
            if (match.Success)
            {
                errors = int.Parse(match.Groups[3].Value);
                warnings = int.Parse(match.Groups[4].Value);
            }

            match = Regex.Match(line, "^([0-9\\.\\-\\:\\[\\]\\s]+)LogInit:Display: LogBlueprint:Error:");
            if (match.Success)
                Tools.WriteColoredLine(line, ConsoleColor.Red);
            else if (verbose)
                Tools.WriteColoredLine(line, ConsoleColor.DarkGray);
        }
    }
}
