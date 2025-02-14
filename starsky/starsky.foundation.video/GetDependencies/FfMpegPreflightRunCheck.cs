using Medallion.Shell;
using starsky.foundation.injection;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfMpegPreflightRunCheck), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegPreflightRunCheck(AppSettings appSettings, IWebLogger logger)
	: IFfMpegPreflightRunCheck
{
	public async Task<bool> TryRun()
	{
		var currentArchitecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();
		return await TryRun(currentArchitecture);
	}

	public async Task<bool> TryRun(string currentArchitecture)
	{
		var exePath = new FfmpegExePath(appSettings).GetExePath(currentArchitecture);

		try
		{
			var result = await Command.Run(exePath, "-version").Task;

			// Check if the command was successful
			if ( result.Success )
			{
				var output = result.StandardOutput;
				if ( output.Contains("ffmpeg", StringComparison.OrdinalIgnoreCase) )
				{
					return true;
				}

				logger.LogError($"[{nameof(FfMpegPreflightRunCheck)}] Invalid application");
			}
			else
			{
				logger.LogError($"[{nameof(FfMpegPreflightRunCheck)}] " +
				                $"Command failed with exit code " +
				                $"{result.ExitCode}: {result.StandardError}");
			}

			return false;
		}
		catch ( Exception exception )
		{
			logger.LogError($"[{nameof(FfMpegPreflightRunCheck)}] " +
			                $"An error occurred while checking FFMpeg: " +
			                $"{exception.Message}");

			return false;
		}
	}
}
