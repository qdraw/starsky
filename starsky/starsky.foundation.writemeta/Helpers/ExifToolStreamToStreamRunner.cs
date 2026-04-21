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
	private const string ExifToolConfigFileName = "exiftool-starsky.config";
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

		var argumentsWithConfig = GetExifToolArgumentsWithConfig(exifToolInputArguments);
		var argumentsWithPipeEnd = $"{argumentsWithConfig} -o - -";

		var memoryStream = new MemoryStream();

		try
		{
			// run with pipes
			var command = Default.Run(_appSettings.ExifToolPath,
					options: opts =>
					{
						opts.StartInfo(si =>
							si.Arguments = argumentsWithPipeEnd);
					})
				< sourceStream > memoryStream;

			var result = await command.Task.ConfigureAwait(false);

			_logger.LogInformation($"[ExifToolRunProcessAsync] {result.Success} ~ exifTool " +
			                       $"{referenceInfoAndPath} {exifToolInputArguments} " +
			                       $"run with result: {result.Success}  ~ ");

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

	internal string GetExifToolArgumentsWithConfig(string exifToolInputArguments)
	{
		var appSettingsDirectory = Path.GetDirectoryName(_appSettings.AppSettingsPath);
		var configBaseDirectory = string.IsNullOrWhiteSpace(appSettingsDirectory)
			? _appSettings.BaseDirectoryProject
			: appSettingsDirectory;
		var configPath = Path.Combine(configBaseDirectory, ExifToolConfigFileName);

		return !File.Exists(configPath)
			? exifToolInputArguments
			: $"-config \"{configPath}\" {exifToolInputArguments}";
	}
}
