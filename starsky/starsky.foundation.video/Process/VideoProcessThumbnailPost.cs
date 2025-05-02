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
		string subPath, string beforeFileHash)
	{
		if ( !runResult.IsSuccess )
		{
			return runResult;
		}

		var tmpPath = GetTempPath(beforeFileHash);
		await _tempStorage.WriteStreamAsync(stream, tmpPath);
		return new VideoResult(true, tmpPath,
			SelectorStorage.StorageServices.Temporary);
	}

	private static string GetTempPath(string beforeFileHash)
	{
		return $"{beforeFileHash}.jpg";
	}
}
