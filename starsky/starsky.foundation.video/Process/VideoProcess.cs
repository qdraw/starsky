using System.ComponentModel;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;
using starsky.foundation.video.Process.Interfaces;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;
using static Medallion.Shell.Shell;

namespace starsky.foundation.video.Process;

public class VideoProcess : IVideoProcess
{
	private readonly ExifCopy _exifCopy;
	private readonly IExifTool _exifTool;
	private readonly IFfMpegDownload _ffMpegDownload;
	private readonly IWebLogger _logger;
	private readonly IStorage _storage;
	private readonly IStorage _thumbnailStorage;

	public VideoProcess(ISelectorStorage selectorStorage, IFfMpegDownload ffMpegDownload,
		IExifTool exifTool, IWebLogger logger, AppSettings appSettings,
		IThumbnailQuery thumbnailQuery)
	{
		_ffMpegDownload = ffMpegDownload;
		_exifTool = exifTool;
		_logger = logger;
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

		_exifCopy = new ExifCopy(_storage,
			_thumbnailStorage, _exifTool, new ReadMeta(_storage,
				appSettings, null!, _logger), thumbnailQuery, _logger);
	}

	public async Task<bool> Run(string subPath,
		string? beforeFileHash, VideoProcessTypes type)
	{
		switch ( type )
		{
			case VideoProcessTypes.Thumbnail:
				var runResult = await Run(subPath, beforeFileHash, "-frames:v 1", "image2", 300000);
				return await PostPrepThumbnail(runResult, subPath, beforeFileHash);
			default:
				return false;
		}
	}

	private async Task<bool> PostPrepThumbnail(bool runResult, string subPath,
		string? beforeFileHash)
	{
		if ( !runResult )
		{
			return false;
		}

		beforeFileHash ??= await FileHash.CalculateHashAsync(_storage.ReadStream(subPath),
			true, CancellationToken.None);

		var readStream = _thumbnailStorage.ReadStream(beforeFileHash);

		var jpegSubPath =
			$"{FilenamesHelper.GetParentPath(subPath)}/{FilenamesHelper.GetFileNameWithoutExtension(subPath)}.jpg";

		await _storage.WriteStreamAsync(readStream, jpegSubPath);

		await _exifCopy.CopyExifPublish(subPath, jpegSubPath);

		return runResult;
	}

	/// <summary>
	///     Run Ffmpeg Command
	/// </summary>
	/// <param name="subPath">where file is located</param>
	/// <param name="beforeFileHash">what is the hash of the orginal file may be null</param>
	/// <param name="ffmpegInputArguments">passed to ffmpeg</param>
	/// <param name="outputFormat">image2 or something else</param>
	/// <param name="maxRead">-1 is entire file, rest is bytes</param>
	/// <param name="cancellationToken">to cancel</param>
	/// <returns></returns>
	private async Task<bool> Run(string subPath, string? beforeFileHash,
		string ffmpegInputArguments,
		string outputFormat, int maxRead,
		CancellationToken cancellationToken = default)
	{
		var downloadStatus = await _ffMpegDownload.DownloadFfMpeg();
		if ( downloadStatus != FfmpegDownloadStatus.Ok )
		{
			_logger.LogDebug("[VideoProcess] FFMpeg download failed");
			return false;
		}

		var sourceStream = _storage.ReadStream(subPath, maxRead);
		beforeFileHash ??= await FileHash.CalculateHashAsync(sourceStream,
			false, cancellationToken);

		var runner =
			new StreamToStreamRunner(_ffMpegDownload.GetSetFfMpegPath(), sourceStream, _logger);
		var (stream, success) =
			await runner.RunProcessAsync(ffmpegInputArguments, outputFormat, subPath);
		if ( !success )
		{
			return false;
		}

		// Only generic updates here, see Thumbnail for more specific updates
		await _thumbnailStorage.WriteStreamAsync(stream, beforeFileHash);
		if ( _thumbnailStorage.Info(beforeFileHash).Size > 100 )
		{
			return true;
		}

		_logger.LogError("[VideoProcess] Thumbnail size is 0");
		_thumbnailStorage.FileDelete(beforeFileHash);
		return false;
	}
}

/// <summary>
///     Handle Ffmpeg Streaming
/// </summary>
internal class StreamToStreamRunner(string ffMpegPath, Stream sourceStream, IWebLogger logger)
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
