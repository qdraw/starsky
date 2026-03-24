using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

public partial class TiffEmbeddedPreviewExtractor
{
	private static async Task<bool> ExtractPreviewToStream(Stream input, PreviewCandidate preview,
		Stream? output)
	{
		if ( !TryValidateJpegOffset(input, preview.Offset, preview.Length) )
		{
			return false;
		}

		if ( output == null )
		{
			return true; // Success, just not saving
		}

		try
		{
			if ( !StreamPrimitives.TrySeek(input, preview.Offset) )
			{
				return false;
			}

			var buffer = ArrayPool<byte>.Shared.Rent(65536);
			try
			{
				var remaining = ( long ) preview.Length;
				while ( remaining > 0 )
				{
					var toRead = ( int ) Math.Min(65536, remaining);
					var read = await input.ReadAsync(buffer.AsMemory(0, toRead));
					if ( read == 0 )
					{
						break;
					}

					await output.WriteAsync(buffer.AsMemory(0, read));
					remaining -= read;
				}

				return remaining == 0;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
		catch
		{
			return false;
		}
	}

	private static bool TryValidateJpegOffset(Stream s, uint offset, uint length)
	{
		if ( offset + length > s.Length )
		{
			return false;
		}

		// Check JPEG SOI marker
		if ( !StreamPrimitives.TrySeek(s, offset) )
		{
			return false;
		}

		Span<byte> marker = stackalloc byte[3];
		if ( s.Read(marker) < 3 )
		{
			return false;
		}

		// JPEG should start with 0xFFD8FF
		return marker[0] == 0xFF && marker[1] == 0xD8 && marker[2] == 0xFF;
	}
}
