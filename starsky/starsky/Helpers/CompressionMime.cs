using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.ResponseCompression;

namespace starsky.Helpers;

internal static class CompressionMime
{
	/// <summary>
	///     application/xhtml+xml image/svg+xml
	/// </summary>
	private static readonly string[] CompressionMimeTypes =
	[
		"application/xhtml+xml",
		"image/svg+xml"
	];

	internal static IEnumerable<string> GetCompressionMimeTypes()
	{
		return ResponseCompressionDefaults.MimeTypes.Concat(CompressionMimeTypes);
	}
}
