using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;
using starsky.foundation.video.Process.Interfaces;

namespace starsky.foundation.video.Process;

[Service(typeof(IVideoProcess), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class VideoProcess : IVideoProcess
{
	private readonly IFfMpegDownload _ffMpegDownload;
	private readonly IFullFilePathExistsService _filePathExistsService;
	private readonly IWebLogger _logger;
	private readonly IStorage _tempStorage;
	private readonly IVideoProcessThumbnailPost _thumbnailPost;

	public VideoProcess(ISelectorStorage selectorStorage, IFfMpegDownload ffMpegDownload,
		IVideoProcessThumbnailPost thumbnailPost, IWebLogger logger,
		IFullFilePathExistsService filePathExistsService)
	{
		_ffMpegDownload = ffMpegDownload;
		_thumbnailPost = thumbnailPost;
		_logger = logger;
		_tempStorage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
		_filePathExistsService = filePathExistsService;
	}

	public async Task<VideoResult> RunVideo(string subPath,
		string beforeFileHash, VideoProcessTypes type)
	{
		switch ( type )
		{
			case VideoProcessTypes.Thumbnail:
				const string ffmpegArguments = "-frames:v 1";
				var (runResult, stream) = await RunFfmpeg(subPath, beforeFileHash, ffmpegArguments,
					"image2");
				var result =
					await _thumbnailPost.PostPrepThumbnail(runResult, stream, subPath,
						beforeFileHash);
				await stream.DisposeAsync();
				return result;
			default:
				return new VideoResult(false, subPath,
					SelectorStorage.StorageServices.SubPath);
		}
	}

	public bool CleanTemporaryFile(string resultResultPath,
		SelectorStorage.StorageServices? resultResultPathType)
	{
		switch ( resultResultPathType )
		{
			case SelectorStorage.StorageServices.Temporary:
				return _tempStorage.FileDelete(resultResultPath);
			default:
				_logger.LogError(
					$"[VideoProcess] CleanTemporaryFile: {resultResultPath} not deleted");
				break;
		}

		return false;
	}

	/// <summary>
	///     Run Ffmpeg Command
	/// </summary>
	/// <param name="subPath">where file is located</param>
	/// <param name="beforeFileHash">used to delete temp file</param>
	/// <param name="ffmpegInputArguments">passed to ffmpeg</param>
	/// <param name="outputFormat">image2 or something else</param>
	/// <returns></returns>
	private async Task<(VideoResult, Stream)> RunFfmpeg(string subPath, string beforeFileHash,
		string ffmpegInputArguments,
		string outputFormat)
	{
		var downloadStatus = await _ffMpegDownload.DownloadFfMpeg();
		if ( downloadStatus != FfmpegDownloadStatus.Ok &&
		     downloadStatus != FfmpegDownloadStatus.OkAlreadyExists )
		{
			_logger.LogDebug("[VideoProcess] FFMpeg download failed");
			return ( new VideoResult(false, null,
				null, "FFMpeg download failed"), Stream.Null );
		}

		var (stream, success) = await RunProcessAsync(subPath, beforeFileHash,
			ffmpegInputArguments,
			outputFormat);
		if ( success )
		{
			return ( new VideoResult(success, subPath), stream );
		}

		_logger.LogError(
			$"[VideoProcess] ({subPath}) Thumbnail generation Failed");
		return (
			new VideoResult(false, subPath, SelectorStorage.StorageServices.SubPath,
				"Generation failed"), Stream.Null );
	}

	private async Task<(Stream, bool)> RunProcessAsync(string subPath, string beforeFileHash,
		string ffmpegInputArguments, string outputFormat)
	{
		var (_, fullFilePath, useTempStorageForInput, fileHashWithExtension) =
			await _filePathExistsService.GetFullFilePath(subPath, beforeFileHash);

		// Run from temp file
		var ffmpegPath = _ffMpegDownload.GetSetFfMpegPath();
		var runner = new FfmpegRunner(ffmpegPath, _logger);
		var result = await runner.RunProcessAsync(fullFilePath,
			ffmpegInputArguments, outputFormat);

		_filePathExistsService.CleanTemporaryFile(fileHashWithExtension, useTempStorageForInput);
		return result;
	}
}
