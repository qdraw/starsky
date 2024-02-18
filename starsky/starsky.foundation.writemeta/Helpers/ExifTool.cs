using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using static Medallion.Shell.Shell;

namespace starsky.foundation.writemeta.Helpers;

/// <summary>
/// Only for writing commands 
/// Check for mapping objects to exifTool commandline args -> 'ExifToolCmdHelper'
/// </summary>
public sealed class ExifTool : IExifTool
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly IStorage _thumbnailStorage;
	private readonly IWebLogger _logger;

	public ExifTool(IStorage sourceStorage, IStorage thumbnailStorage, AppSettings appSettings,
		IWebLogger logger)
	{
		_appSettings = appSettings;
		_iStorage = sourceStorage;
		_thumbnailStorage = thumbnailStorage;
		_logger = logger;
	}

	/// <summary>
	/// Write commands to ExifTool for ReadStream
	/// </summary>
	/// <param name="subPath">the location</param>
	/// <param name="beforeFileHash">thumbnail fileHash</param>
	/// <param name="command">exifTool command line args</param>
	/// <param name="cancellationToken">to cancel</param>
	/// <returns>true=success, newFileHash</returns>
	public async Task<KeyValuePair<bool, string>> WriteTagsAndRenameThumbnailAsync(
		string subPath,
		string? beforeFileHash, string command, CancellationToken cancellationToken = default)
	{
		return await WriteTagsAndRenameThumbnailInternalAsync(subPath, beforeFileHash, command,
			cancellationToken);
	}

	[SuppressMessage("ReSharper", "InvertIf")]
	[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
	private async Task<KeyValuePair<bool, string>>
		WriteTagsAndRenameThumbnailInternalAsync(string subPath,
			string? beforeFileHash, string command,
			CancellationToken cancellationToken = default)
	{
		var sourceStream = _iStorage.ReadStream(subPath);
		beforeFileHash ??=
			await FileHash.CalculateHashAsync(sourceStream, false, cancellationToken);

		var runner = new StreamToStreamRunner(_appSettings, sourceStream, _logger);
		var stream = await runner.RunProcessAsync(command, subPath);

		var newHashCode = await RenameThumbnailByStream(beforeFileHash, stream,
			!beforeFileHash.Contains(FileHash.GeneratedPostFix), cancellationToken);

		if ( stream.Length <= 15 &&
		     ( await StreamToStringHelper.StreamToStringAsync(stream, false) )
		     .Contains("Fake ExifTool", StringComparison.InvariantCultureIgnoreCase) )
		{
			_logger.LogError(
				$"[WriteTagsAndRenameThumbnailAsync] Fake Exiftool detected {subPath}");
			return new KeyValuePair<bool, string>(false, beforeFileHash);
		}

		// Need to Dispose for Windows
		await sourceStream.DisposeAsync();

		stream.Seek(0, SeekOrigin.Begin);
		var streamResult = await _iStorage.WriteStreamAsync(stream, subPath);

		return new KeyValuePair<bool, string>(streamResult, newHashCode);
	}


	/// <summary>
	/// Need to dispose string afterwards yourself
	/// </summary>
	/// <param name="beforeFileHash">the before fileHash</param>
	/// <param name="stream">stream</param>
	/// <param name="isSuccess">isHashing success, otherwise skip this</param>
	/// <param name="cancellationToken">cancel Token</param>
	/// <returns></returns>
	[SuppressMessage("ReSharper", "MustUseReturnValue")]
	internal async Task<string> RenameThumbnailByStream(
		string beforeFileHash, Stream stream, bool isSuccess,
		CancellationToken cancellationToken = default)
	{
		if ( string.IsNullOrEmpty(beforeFileHash) || !isSuccess )
		{
			return string.Empty;
		}

		var fileHashStream = await StreamGetFirstBytes.GetFirstBytesAsync(stream,
			FileHash.MaxReadSize,
			cancellationToken);

		var newHashCode =
			await FileHash.CalculateHashAsync(fileHashStream, true, cancellationToken);

		if ( string.IsNullOrEmpty(newHashCode) )
		{
			return string.Empty;
		}

		if ( beforeFileHash == newHashCode )
		{
			return newHashCode;
		}

		var service = new ThumbnailFileMoveAllSizes(_thumbnailStorage);
		service.FileMove(beforeFileHash, newHashCode);

		return newHashCode;
	}

	/// <summary>
	/// Write commands to ExifTool for ReadStream
	/// </summary>
	/// <param name="subPath">the location</param>
	/// <param name="command">exifTool command line args</param>
	/// <returns>true=success</returns>
	public async Task<bool> WriteTagsAsync(string subPath, string command)
	{
		var inputStream = _iStorage.ReadStream(subPath);

		var runner = new StreamToStreamRunner(_appSettings, inputStream, _logger);
		var stream = await runner.RunProcessAsync(command);

		var isWritten = await _iStorage.WriteStreamAsync(stream, subPath);

		// Need to Dispose for Windows
		await inputStream.DisposeAsync();

		return isWritten;
	}

	/// <summary>
	/// Write commands to ExifTool for ThumbnailWriteStream (Does NOT work with mono/legacy)
	/// </summary>
	/// <param name="fileHash">the location</param>
	/// <param name="command">exifTool command line args</param>
	/// <returns>true=success</returns>
	public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
	{
		var inputStream = _thumbnailStorage.ReadStream(fileHash);
		var runner = new StreamToStreamRunner(_appSettings, inputStream, _logger);
		var stream = await runner.RunProcessAsync(command);
		// Need to Close/Dispose for Windows and needs before WriteStreamAsync
		inputStream.Close();
		return await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
	}
}

/// <summary>
/// Handle ExifTool Streaming
/// </summary>
internal class StreamToStreamRunner
{
	private readonly Stream _sourceStream;
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;

	public StreamToStreamRunner(AppSettings appSettings, Stream sourceStream, IWebLogger logger)
	{
		_sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
		_appSettings = appSettings;
		_logger = logger;
	}

	/// <summary>
	/// Run Command async (and keep stream open)
	/// </summary>
	/// <param name="exifToolInputArguments">exifTool args</param>
	/// <param name="referencePath">reference path (only for display)</param>
	/// <returns>bool if success</returns>
	/// <exception cref="ArgumentException">if exifTool is missing</exception>
	public async Task<Stream> RunProcessAsync(string exifToolInputArguments,
		string referencePath = "")
	{
		var argumentsWithPipeEnd = $"{exifToolInputArguments} -o - -";

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
				< _sourceStream > memoryStream;

			var result = await command.Task.ConfigureAwait(false);

			_logger.LogInformation($"[RunProcessAsync] {result.Success} ~ exifTool " +
			                       $"{referencePath} {exifToolInputArguments} " +
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
}
