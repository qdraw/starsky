using System;
using System.Collections.Generic;
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

namespace starsky.foundation.writemeta.Helpers;

/// <summary>
///     Only for writing commands
///     Check for mapping objects to exifTool commandline args -> 'ExifToolCmdHelper'
/// </summary>
public sealed class ExifTool : IExifTool
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IStorage _thumbnailStorage;

	public ExifTool(IStorage sourceStorage, IStorage thumbnailStorage, AppSettings appSettings,
		IWebLogger logger)
	{
		_appSettings = appSettings;
		_iStorage = sourceStorage;
		_thumbnailStorage = thumbnailStorage;
		_logger = logger;
	}

	/// <summary>
	///     Write commands to ExifTool for ReadStream
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

	/// <summary>
	///     Write commands to ExifTool for ReadStream
	/// </summary>
	/// <param name="subPath">the location</param>
	/// <param name="command">exifTool command line args</param>
	/// <returns>true=success</returns>
	public async Task<bool> WriteTagsAsync(string subPath, string command)
	{
		_logger.LogInformation($"[WriteTagsAsync] Next update for {subPath}");

		var inputStream = _iStorage.ReadStream(subPath);

		var runner = new ExifToolStreamToStreamRunner(_appSettings, _logger);
		var stream = await runner.RunProcessAsync(inputStream, command, subPath);

		// Need to Dispose for Windows
		// inputStream is disposed
		await inputStream.DisposeAsync();

		return await _iStorage.WriteStreamAsync(stream, subPath);
	}

	/// <summary>
	///     Write commands to ExifTool for ThumbnailWriteStream (Does NOT work with mono/legacy)
	/// </summary>
	/// <param name="fileHash">the location</param>
	/// <param name="command">exifTool command line args</param>
	/// <returns>true=success</returns>
	public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
	{
		var inputStream = _thumbnailStorage.ReadStream(fileHash);
		var runner = new ExifToolStreamToStreamRunner(_appSettings, _logger);
		var stream = await runner.RunProcessAsync(inputStream, command, fileHash);
		// Need to Close/Dispose for Windows and needs before WriteStreamAsync
		await inputStream.DisposeAsync();
		return await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
	}

	[SuppressMessage("ReSharper", "InvertIf")]
	[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
	private async Task<KeyValuePair<bool, string>>
		WriteTagsAndRenameThumbnailInternalAsync(string subPath,
			string? beforeFileHash, string command,
			CancellationToken cancellationToken = default)
	{
		_logger.LogInformation($"[WriteTagsAndRenameThumbnailAsync] Update: {subPath}");

		var sourceStream = _iStorage.ReadStream(subPath);
		beforeFileHash ??= await FileHash.CalculateHashAsync(sourceStream,
			false, cancellationToken);

		var runner = new ExifToolStreamToStreamRunner(_appSettings, _logger);
		var stream = await runner.RunProcessAsync(sourceStream, command, subPath);

		// Need to Close / Dispose for Windows
		sourceStream.Close();
		await sourceStream.DisposeAsync();

		var newHashCode = await RenameThumbnailByStream(beforeFileHash, stream,
			!beforeFileHash.Contains(FileHash.GeneratedPostFix), cancellationToken);

		if ( stream.Length <= 15 &&
		     ( await StreamToStringHelper.StreamToStringAsync(stream, false) )
		     .Contains("Fake ExifTool", StringComparison.InvariantCultureIgnoreCase) )
		{
			await stream.DisposeAsync();
			_logger.LogError(
				$"[WriteTagsAndRenameThumbnailAsync] Fake Exiftool detected {subPath}");
			return new KeyValuePair<bool, string>(false, beforeFileHash);
		}

		stream.Seek(0, SeekOrigin.Begin);
		var streamResult = await _iStorage.WriteStreamAsync(stream, subPath);

		return new KeyValuePair<bool, string>(streamResult, newHashCode);
	}


	/// <summary>
	///     Need to dispose string afterward yourself
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

		var service = new ThumbnailFileMoveAllSizes(_thumbnailStorage, _appSettings);
		service.FileMove(beforeFileHash, newHashCode);

		return newHashCode;
	}
}
