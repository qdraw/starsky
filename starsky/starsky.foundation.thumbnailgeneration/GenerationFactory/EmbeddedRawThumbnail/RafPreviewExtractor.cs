using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

internal sealed class RafPreviewExtractor(IWebLogger logger)
{
	private static readonly byte[] Signature = "FUJIFILMCCD-RAW "u8.ToArray();

	public Task<bool> TryExtract(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		using var stream = new FileStream(rawFilePath, FileMode.Open, FileAccess.Read,
			FileShare.Read);
		if ( stream.Length < 96 )
		{
			return Task.FromResult(false);
		}

		Span<byte> signature = stackalloc byte[16];
		if ( stream.Read(signature) != 16 || !signature.SequenceEqual(Signature) )
		{
			return Task.FromResult(false);
		}

		stream.Seek(84, SeekOrigin.Begin);
		Span<byte> previewMeta = stackalloc byte[8];
		if ( stream.Read(previewMeta) != 8 )
		{
			return Task.FromResult(false);
		}

		var offset = BinaryPrimitives.ReadUInt32BigEndian(previewMeta[..4]);
		var length = BinaryPrimitives.ReadUInt32BigEndian(previewMeta[4..8]);
		if ( offset == 0 || length == 0 )
		{
			return Task.FromResult(false);
		}

		var end = (long)offset + length;
		if ( end > stream.Length )
		{
			return Task.FromResult(false);
		}

		return Task.FromResult(new JpegSegmentScanner(logger).TryExtract(rawFilePath,
			[(offset, length)], outputLargePath, outputMediumPath));
	}
}

