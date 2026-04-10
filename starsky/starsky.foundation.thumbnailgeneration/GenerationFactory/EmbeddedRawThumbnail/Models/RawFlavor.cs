using System.IO;
using System.Runtime.CompilerServices;
using starsky.foundation.platform.Helpers;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

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

	public static RawFlavor GetRawFlavorFromImageFormat(ExtensionRolesHelper.ImageFormat? imageFormat)
	{
		return imageFormat switch
		{
			ExtensionRolesHelper.ImageFormat.arw => RawFlavor.SonyArw,
			ExtensionRolesHelper.ImageFormat.cr2 => RawFlavor.CanonCr2,
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
