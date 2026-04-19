using System;
using System.IO;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class JpegContainerFallback
{
	private const int MinJpegBytes = 4096;

	internal static bool TryExtractLargestJpeg(Stream input, Stream output, out string error)
	{
		error = string.Empty;
		if ( !input.CanSeek )
		{
			error = "Input stream is not seekable";
			return false;
		}

		try
		{
			input.Seek(0, SeekOrigin.Begin);
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			var data = ms.ToArray();

			var bestStart = -1;
			var bestLength = 0;

			for ( var i = 0; i < data.Length - 2; i++ )
			{
				if ( data[i] != 0xFF || data[i + 1] != 0xD8 || data[i + 2] != 0xFF )
				{
					continue;
				}

				for ( var j = i + 3; j < data.Length - 1; j++ )
				{
					if ( data[j] != 0xFF || data[j + 1] != 0xD9 )
					{
						continue;
					}

					var length = j + 2 - i;
					if ( length >= MinJpegBytes && length > bestLength )
					{
						bestStart = i;
						bestLength = length;
					}

					break;
				}
			}

			if ( bestStart < 0 )
			{
				error = "No embedded JPEG found in container stream";
				return false;
			}

			if ( output.CanSeek )
			{
				output.SetLength(0);
				output.Seek(0, SeekOrigin.Begin);
			}

			output.Write(data, bestStart, bestLength);
			if ( output.CanSeek )
			{
				output.Seek(0, SeekOrigin.Begin);
			}

			return true;
		}
		catch ( Exception ex )
		{
			error = ex.Message;
			return false;
		}
	}
}

