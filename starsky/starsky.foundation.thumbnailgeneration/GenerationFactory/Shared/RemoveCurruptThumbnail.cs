using System.Threading.Tasks;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;

public class RemoveCorruptThumbnail(ISelectorStorage selectorStorage)
{
	private readonly IStorage _storage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage _thumbnailStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	public bool RemoveIfCorrupt(string outputFileHashWithExtension)
	{
		if ( _thumbnailStorage.Info(outputFileHashWithExtension).Size > 10 )
		{
			return false;
		}

		_thumbnailStorage.FileDelete(outputFileHashWithExtension);
		return true;
	}

	public async Task WriteErrorMessageToBlockLog(string subPath, string errorMessage)
	{
		var stream = StringToStreamHelper.StringToStream("Thumbnail error " + errorMessage);
		await _storage.WriteStreamAsync(stream,
			ErrorLogItemFullPath.GetErrorLogItemFullPath(subPath));
	}
}
