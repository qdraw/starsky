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
		var inputStream = _iStorage.ReadStream(subPath);
		beforeFileHash ??= await FileHash.CalculateHashAsync(inputStream, false, cancellationToken);

		var runner = new StreamToStreamRunner(_appSettings, inputStream, _logger);
		var stream = await runner.RunProcessAsync(command);

		var newHashCode = await RenameThumbnailByStream(beforeFileHash, stream,
			!beforeFileHash.Contains(FileHash.GeneratedPostFix), cancellationToken);

		stream.Seek(0, SeekOrigin.Begin);

		// Need to Dispose for Windows
		inputStream.Close();

		if ( stream.Length <= 15 && ( await StreamToStringHelper.StreamToStringAsync(stream, true) )
			.Contains("Fake ExifTool", StringComparison.InvariantCultureIgnoreCase) )
		{
			_logger.LogError(
				$"[WriteTagsAndRenameThumbnailAsync] Fake Exiftool detected {subPath}");
			return new KeyValuePair<bool, string>(false, beforeFileHash);
		}

		return new KeyValuePair<bool, string>(await _iStorage.WriteStreamAsync(stream, subPath),
			newHashCode);
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

		var buffer = new byte[FileHash.MaxReadSize];
		await stream.ReadAsync(buffer.AsMemory(0, FileHash.MaxReadSize), cancellationToken);

		var newHashCode =
			await FileHash.CalculateHashAsync(new MemoryStream(buffer), true, cancellationToken);
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

		// Need to Dispose for Windows
		inputStream.Close();
		return await _iStorage.WriteStreamAsync(stream, subPath);
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
		// Need to Dispose for Windows
		inputStream.Close();
		return await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
	}
}

/// <summary>
/// Handle ExifTool Streaming
/// </summary>
internal class StreamToStreamRunner
{
	private readonly Stream _src;
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;

	public StreamToStreamRunner(AppSettings appSettings, Stream src, IWebLogger logger)
	{
		_src = src ?? throw new ArgumentNullException(nameof(src));
		_appSettings = appSettings;
		_logger = logger;
	}

	/// <summary>
	/// Run Command async (and keep stream open)
	/// </summary>
	/// <param name="exifToolInputArguments">exifTool args</param>
	/// <returns>bool if success</returns>
	/// <exception cref="ArgumentException">if exifTool is missing</exception>
	public async Task<Stream> RunProcessAsync(string exifToolInputArguments)
	{
		var argumentsWithPipeEnd = $"{exifToolInputArguments} -o - -";

		var memoryStream = new MemoryStream();

		try
		{
			// run with pipes
			var command = Default.Run(_appSettings.ExifToolPath,
				options:
				opts => { opts.StartInfo(si => si.Arguments = argumentsWithPipeEnd); });

			command.RedirectFrom(_src);
			command.RedirectTo(memoryStream);

			var result = await command.Task;

			if ( _appSettings.IsVerbose() )
			{
				_logger.LogInformation($"[RunProcessAsync] ~ exifTool {exifToolInputArguments} " +
									   $"run with result: {result.Success} ~ ");
			}

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
