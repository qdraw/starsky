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
	private readonly IStorage _temporaryStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
	
	private VideoResult _result = new(true, string.Empty,
		SelectorStorage.StorageServices.Temporary, "Mocked");

	public async Task<VideoResult> RunVideo(string subPath, string? beforeFileHash,
		VideoProcessTypes type)
	{
		ArgumentNullException.ThrowIfNull(beforeFileHash);

		var thumbnailName = ThumbnailNameHelper.Combine(beforeFileHash, ThumbnailSize.ExtraLarge,
			ThumbnailImageFormat.jpg);

		await _thumbnailStorage.WriteStreamAsync(new MemoryStream([.. CreateAnImage.Bytes]),
			thumbnailName
		);

		_result.ResultPath = thumbnailName;
		return _result;
	}

	public void CleanTemporaryFile(string resultResultPath,
		SelectorStorage.StorageServices? resultResultPathType)
	{
		if ( _temporaryStorage.ExistFile(resultResultPath) )
		{
			_temporaryStorage.FileDelete(resultResultPath);
		}
	}

	public void SetSuccessResult()
	{
		_result = new VideoResult(true, string.Empty,
			SelectorStorage.StorageServices.Temporary,
			"Mocked");
	}

	public void SetFailureResult(string errorMessage)
	{
		_result = new VideoResult { IsSuccess = false, ErrorMessage = $"{errorMessage} Mocked" };
	}
}
