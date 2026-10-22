namespace Starsky.Desktop.Services;

internal record ProcessResult(int ExitCode, string Output);

internal interface IProcessRunner
{
    ProcessResult Run(string fileName, string args);
    ProcessResult RunElevated(string fileName, string args);
}
