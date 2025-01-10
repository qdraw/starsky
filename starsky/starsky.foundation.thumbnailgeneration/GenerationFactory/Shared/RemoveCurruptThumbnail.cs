using System;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;

public class RemoveCorruptThumbnail(IStorage thumbnailStorage)
{
	public void RemoveAndThrow(string outputFileHashWithExtension)
	{
		if ( thumbnailStorage.Info(outputFileHashWithExtension).Size > 10 )
		{
			return;
		}

		thumbnailStorage.FileDelete(outputFileHashWithExtension);
		throw new BadImageFormatException("Image is corrupt");
	}
}
