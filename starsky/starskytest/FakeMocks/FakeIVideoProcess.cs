using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.Process;
using starsky.foundation.video.Process.Interfaces;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks;

public class FakeIVideoProcess(ISelectorStorage selectorStorage) : IVideoProcess
{
	private readonly IStorage _thumbnailStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	public async Task<VideoResult> RunVideo(string subPath, string? beforeFileHash,
		VideoProcessTypes type)
	{
		ArgumentNullException.ThrowIfNull(beforeFileHash);

		var thumbnailName = ThumbnailNameHelper.Combine(beforeFileHash, ThumbnailSize.ExtraLarge,
			ThumbnailImageFormat.jpg);
		
		await _thumbnailStorage.WriteStreamAsync(new MemoryStream([.. CreateAnImage.Bytes]),
			thumbnailName
		);
		
		return new VideoResult(true, thumbnailName, "Mocked");
	}
}
