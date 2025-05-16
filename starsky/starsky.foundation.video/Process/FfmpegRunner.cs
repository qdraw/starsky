using System.ComponentModel;
using Medallion.Shell;
using starsky.foundation.platform.Interfaces;
using static Medallion.Shell.Shell;

namespace starsky.foundation.video.Process;

public class FfmpegRunner(string ffMpegPath, IWebLogger logger)
{
	public async Task<(Stream, bool)> RunProcessAsync(string fullFilePath,
		string ffmpegInputArguments, string format)
	{
		ArgumentNullException.ThrowIfNull(fullFilePath);

		var argumentsWithPipeEnd =
			$"-i \"{fullFilePath}\" {ffmpegInputArguments} -f {format} pipe:1";

		var memoryStream = new MemoryStream();

		try
		{
			var command = Default.Run(ffMpegPath,
				options: opts =>
				{
					opts.StartInfo(si =>
						si.Arguments = argumentsWithPipeEnd);
				}) > memoryStream;

			var result = await command.Task;

			await LogInformationOrError(command, result, argumentsWithPipeEnd);

			memoryStream.Seek(0, SeekOrigin.Begin);

			return ( memoryStream, result.Success );
		}
		catch ( Win32Exception exception )
		{
			throw new ArgumentException("Error when trying to start the ffmpeg process.  " +
			                            "Please make sure ffmpeg is installed, and its path is properly " +
			                            "specified in the options. " +
			                            argumentsWithPipeEnd, exception);
		}
	}

	private async Task LogInformationOrError(Command command, CommandResult result,
		string argumentsWithPipeEnd)
	{
		if ( !result.Success )
		{
			var error = await command.StandardError.ReadToEndAsync();

			logger.LogError($"[FfmpegRunProcessAsync] {result.Success} ~ ffmpeg " +
			                $"{argumentsWithPipeEnd} " +
			                $"run with result: \n {error}  ~ ");
			return;
		}

		logger.LogInformation($"[FfmpegRunProcessAsync] {result.Success} ~ ffmpeg " +
		                      $"{argumentsWithPipeEnd} " +
		                      $"run with result: {result.Success}  ~ ");
	}
}
