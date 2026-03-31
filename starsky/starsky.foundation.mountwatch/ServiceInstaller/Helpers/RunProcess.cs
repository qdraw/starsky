using System.Diagnostics;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

internal class RunProcess(IWebLogger logger)
{
	/// <summary>
	///     Run an external process and return whether it succeeded
	/// </summary>
	internal async Task<bool> RunProcessAsync(string fileName, string arguments)
	{
		var processInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var process = Process.Start(processInfo);
		if ( process == null )
		{
			return false;
		}

		var output = await process.StandardOutput.ReadToEndAsync();
		var error = await process.StandardError.ReadToEndAsync();

		await process.WaitForExitAsync();

		if ( process.ExitCode != 0 )
		{
			logger.LogError(
				$"Process {fileName} {arguments} failed with exit code {process.ExitCode}\nOutput: {output}\nError: {error}");
		}

		return process.ExitCode == 0;
	}
}
