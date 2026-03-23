using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Extracts embedded JPEG previews from Fujifilm RAF containers.
///     Selection prefers JPEG candidates with IPTC APP13 metadata.
/// </summary>
public class RafPreviewExtractor(IWebLogger logger, ISelectorStorage selectorStorage)
{
	private IStorage subPathStorage => selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	private IStorage tempStorage => selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	public async Task<bool> TryExtract(string subPathRawFile, string? outputLargePath)
	{
		if ( !subPathStorage.ExistFile(subPathRawFile) )
		{
			return false;
		}

		try
		{
			await using var input = subPathStorage.ReadStream(subPathRawFile);
			await using var output = new MemoryStream();

			var ok = await ContainerJpegScanner.TryExtractBestPreview(input, output);
			if ( !ok || outputLargePath == null || output.Length == 0 )
			{
				return ok;
			}

			output.Seek(0, SeekOrigin.Begin);
			return await tempStorage.WriteStreamAsync(output, outputLargePath);
		}
		catch ( Exception exception )
		{
			logger.LogDebug(
				$"[RafPreviewExtractor] Failed to extract from {subPathRawFile}: {exception.Message}");
			return false;
		}
	}
}
