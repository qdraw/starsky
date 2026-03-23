using System.IO;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

internal static class RawFlavorHelper
{
	internal static RawFlavor GetRawFlavorFromPath(string pathOrReference)
	{
		var ext = Path.GetExtension(pathOrReference).ToLowerInvariant();
		return ext switch
		{
			".arw" => RawFlavor.SonyArw,
			".cr2" => RawFlavor.CanonCr2,
			_ => RawFlavor.Unknown
		};
	}
}

internal enum RawFlavor
{
	Unknown,
	SonyArw,
	CanonCr2
}
