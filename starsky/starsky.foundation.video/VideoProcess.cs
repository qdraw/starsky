using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video;

public class VideoProcess
{
	private readonly IFfMpegDownload _ffMpegDownload;

	public VideoProcess(IFfMpegDownload ffMpegDownload)
	{
		_ffMpegDownload = ffMpegDownload;
	}
	
	public async Task Run()
	{
		var downloadStatus = await _ffMpegDownload.DownloadFfMpeg();
		if ( downloadStatus != FfmpegDownloadStatus.Ok )
		{
			return;
		}
		// Do something with the downloaded FFMpeg
	}

}
