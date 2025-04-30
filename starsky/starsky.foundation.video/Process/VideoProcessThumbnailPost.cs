using starsky.foundation.injection;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.Process.Interfaces;

namespace starsky.foundation.video.Process;

[Service(typeof(IVideoProcessThumbnailPost), InjectionLifetime = InjectionLifetime.Scoped)]
public class VideoProcessThumbnailPost(ISelectorStorage selectorStorage)
	: IVideoProcessThumbnailPost
{
	private readonly IStorage _tempStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	public async Task<VideoResult> PostPrepThumbnail(VideoResult runResult,
		Stream stream,
		string subPath)
	{
		if ( !runResult.IsSuccess )
		{
			return runResult;
		}

		var tmpPath = await WriteStreamInTempFolder(stream);
		return new VideoResult(true, tmpPath);
	}

	private async Task<string> WriteStreamInTempFolder(Stream stream)
	{
		var tmpPath = $"{Guid.NewGuid():N}.jpg";
		await _tempStorage.WriteStreamAsync(stream, tmpPath);
		return tmpPath;
	}
}
