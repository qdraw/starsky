using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

public class JpegExifPreviewExtractor(IWebLogger logger, ISelectorStorage selectorStorage)
{
	private readonly IStorage _subPathStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage _tempStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Temporary);


	public async Task<bool> TryExtract(string subPathRawFile,
		string? outputLargePath)
	{
		if ( !_subPathStorage.ExistFile(subPathRawFile) )
		{
			return false;
		}

		try
		{
			await using var input = _subPathStorage.ReadStream(subPathRawFile);
			await using var output = new MemoryStream();

			var ok = await JpegExtractPreviewHelper.TryExtractFromStream(input, output);
			if ( !ok || outputLargePath == null )
			{
				return ok;
			}

			if ( output.Length == 0 )
			{
				return false;
			}

			output.Seek(0, SeekOrigin.Begin);
			return await _tempStorage.WriteStreamAsync(output, outputLargePath);
		}
		catch ( Exception ex )
		{
			logger.LogError(
				$"[JpegExifPreviewExtractor] Failed to extract from {subPathRawFile}: {ex.Message}");
			return false;
		}
	}
}
