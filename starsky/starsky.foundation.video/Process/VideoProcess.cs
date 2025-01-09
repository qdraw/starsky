using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;
using starsky.foundation.video.Process.Interfaces;

namespace starsky.foundation.video.Process;

[Service(typeof(IVideoProcess), InjectionLifetime = InjectionLifetime.Scoped)]
public class VideoProcess : IVideoProcess
{
	private readonly IFfMpegDownload _ffMpegDownload;
	private readonly IWebLogger _logger;
	private readonly IStorage _storage;
	private readonly IVideoProcessThumbnailPost _thumbnailPost;

	public VideoProcess(ISelectorStorage selectorStorage, IFfMpegDownload ffMpegDownload,
		IVideoProcessThumbnailPost thumbnailPost, IWebLogger logger)
	{
		_ffMpegDownload = ffMpegDownload;
		_thumbnailPost = thumbnailPost;
		_logger = logger;
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	}

	public async Task<VideoResult> RunVideo(string subPath,
		string? beforeFileHash, VideoProcessTypes type)
	{
		switch ( type )
		{
			case VideoProcessTypes.Thumbnail:
				var (runResult, stream) = await RunFfmpeg(subPath,
					"-frames:v 1", "image2", 300000);
				return await _thumbnailPost.PostPrepThumbnail(runResult, stream, subPath);
			default:
				return new VideoResult(false, subPath);
		}
	}

	/// <summary>
	///     Run Ffmpeg Command
	/// </summary>
	/// <param name="subPath">where file is located</param>
	/// <param name="ffmpegInputArguments">passed to ffmpeg</param>
	/// <param name="outputFormat">image2 or something else</param>
	/// <param name="maxRead">-1 is entire file, rest is bytes</param>
	/// <returns></returns>
	private async Task<(VideoResult, Stream)> RunFfmpeg(string subPath,
		string ffmpegInputArguments,
		string outputFormat, int maxRead)
	{
		var downloadStatus = await _ffMpegDownload.DownloadFfMpeg();
		if ( downloadStatus != FfmpegDownloadStatus.Ok )
		{
			_logger.LogDebug("[VideoProcess] FFMpeg download failed");
			return ( new VideoResult(false, null, "FFMpeg download failed"), Stream.Null );
		}

		var sourceStream = _storage.ReadStream(subPath, maxRead);

		var runner =
			new FfmpegStreamToStreamRunner(_ffMpegDownload.GetSetFfMpegPath(), sourceStream,
				_logger);
		var (stream, success) =
			await runner.RunProcessAsync(ffmpegInputArguments, outputFormat, subPath);

		return ( new VideoResult(success, subPath), stream );
	}
}
