using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using static Medallion.Shell.Shell;

namespace starsky.foundation.writemeta.Helpers;

/// <summary>
///     Handle ExifTool Streaming
/// </summary>
public class ExifToolStreamToStreamRunner
{
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;

	public ExifToolStreamToStreamRunner(AppSettings appSettings,
		IWebLogger logger)
	{
		_appSettings = appSettings;
		_logger = logger;
	}

	/// <summary>
	///     Run Command async (and keep stream open)
	/// </summary>
	/// <param name="sourceStream">source image</param>
	/// <param name="exifToolInputArguments">exifTool args</param>
	/// <param name="referenceInfoAndPath">reference path (only for display)</param>
	/// <returns>bool if success</returns>
	/// <exception cref="ArgumentException">if exifTool is missing</exception>
	public async Task<Stream> RunProcessAsync(Stream sourceStream, string exifToolInputArguments,
		string referenceInfoAndPath = "")
	{
		ArgumentNullException.ThrowIfNull(sourceStream);

		_logger.LogDebug(
			$"info: {sourceStream.CanRead}  {sourceStream.CanSeek}  {sourceStream.CanWrite}" +
			$" {sourceStream.Position}");

		var argumentsWithPipeEnd = $"{exifToolInputArguments} -o - -";

		var memoryStream = new MemoryStream();

		try
		{
			var resultSuccess = await RunCommandAsync(sourceStream, memoryStream,
				argumentsWithPipeEnd).ConfigureAwait(false);

			_logger.LogInformation($"[ExifToolRunProcessAsync] {resultSuccess} ~ exifTool " +
			                       $"{referenceInfoAndPath} {exifToolInputArguments} " +
			                       $"run with result: {resultSuccess}  ~ ");

			memoryStream.Seek(0, SeekOrigin.Begin);

			return memoryStream;
		}
		catch ( IOException ex ) when (
			ex.Message.Contains("pipe is being closed", StringComparison.OrdinalIgnoreCase) ||
			ex.Message.Contains("Broken pipe", StringComparison.OrdinalIgnoreCase) )
		{
			// Process exited before stdin was fully consumed — normal when a fast-exit process
			// ignores remaining input. The stdout data already written to memoryStream is valid.
			_logger.LogDebug(
				$"[ExifToolRunProcessAsync] Stdin pipe closed early for {referenceInfoAndPath}");
			memoryStream.Seek(0, SeekOrigin.Begin);
			return memoryStream;
		}
		catch ( Win32Exception exception )
		{
			throw new ArgumentException("Error when trying to start the exifTool process.  " +
			                            "Please make sure exifTool is installed, and its path is properly " +
			                            "specified in the options.", exception);
		}
	}

	protected virtual async Task<bool> RunCommandAsync(Stream sourceStream, Stream outputStream,
		string arguments)
	{
		var command = Default.Run(_appSettings.ExifToolPath,
				options: opts =>
				{
					opts.StartInfo(si => si.Arguments = arguments);
				})
			< sourceStream > outputStream;

		var result = await command.Task.ConfigureAwait(false);
		return result.Success;
	}
}
