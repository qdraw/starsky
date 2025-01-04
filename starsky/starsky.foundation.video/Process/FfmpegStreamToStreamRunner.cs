using System.ComponentModel;
using starsky.foundation.platform.Interfaces;
using static Medallion.Shell.Shell;

namespace starsky.foundation.video.Process;

/// <summary>
///     Handle Ffmpeg Streaming
/// </summary>
internal class FfmpegStreamToStreamRunner(string ffMpegPath, Stream sourceStream, IWebLogger logger)
{
	private readonly Stream _sourceStream =
		sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));

	/// <summary>
	///     Run Command async (and keep stream open)
	/// </summary>
	/// <param name="ffmpegInputArguments">ffmpeg args</param>
	/// <param name="referenceInfoAndPath">reference path (only for display)</param>
	/// <param name="format">output format</param>
	/// <returns>bool if success</returns>
	/// <exception cref="ArgumentException">if exifTool is missing</exception>
	public async Task<(Stream, bool)> RunProcessAsync(string ffmpegInputArguments, string format,
		string referenceInfoAndPath = "")
	{
		var argumentsWithPipeEnd = $"-i pipe:0 {ffmpegInputArguments} -f {format} -";

		var memoryStream = new MemoryStream();

		try
		{
			// run with pipes
			var command = Default.Run(ffMpegPath,
					options: opts =>
					{
						opts.StartInfo(si =>
							si.Arguments = argumentsWithPipeEnd);
					})
				< _sourceStream > memoryStream;

			var result = await command.Task.ConfigureAwait(false);

			if ( !result.Success )
			{
				var error = await command.StandardError.ReadToEndAsync();
				logger.LogError("[RunProcessAsync] ffmpeg " + error);
			}

			logger.LogInformation($"[RunProcessAsync] {result.Success} ~ ffmpeg " +
			                      $"{referenceInfoAndPath} {ffmpegInputArguments} " +
			                      $"run with result: {result.Success}  ~ ");

			memoryStream.Seek(0, SeekOrigin.Begin);

			return ( memoryStream, result.Success );
		}
		catch ( Win32Exception exception )
		{
			throw new ArgumentException("Error when trying to start the ffmpeg process.  " +
			                            "Please make sure ffmpeg is installed, and its path is properly " +
			                            "specified in the options.", exception);
		}
	}
}
