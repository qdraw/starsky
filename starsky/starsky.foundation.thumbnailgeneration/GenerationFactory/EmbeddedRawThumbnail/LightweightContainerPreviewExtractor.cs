using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

internal class LightweightContainerPreviewExtractor(IWebLogger logger)
{
	public Task<bool> TryExtract(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		var fileLength = new FileInfo(rawFilePath).Length;
		var ranges = new List<(long Offset, long Length)>
		{
			(0L, fileLength)
		};

		var scanner = new JpegSegmentScanner(logger);
		return Task.FromResult(scanner.TryExtract(rawFilePath, ranges, outputLargePath,
			outputMediumPath));
	}
}

