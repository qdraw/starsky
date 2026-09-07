using System.Diagnostics;

namespace Starsky.Desktop.Services;

internal sealed class WindowsProcessRunner : IProcessRunner
{
    public ProcessResult Run(string fileName, string args)
    {
        var psi = new ProcessStartInfo(fileName, args)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        if (process == null) return new ProcessResult(-1, string.Empty);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, output);
    }

    public ProcessResult RunElevated(string fileName, string args)
    {
        var psi = new ProcessStartInfo(fileName, args)
        {
            Verb = "runas",
            UseShellExecute = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        if (process == null) return new ProcessResult(-1, string.Empty);
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, string.Empty);
    }
}
