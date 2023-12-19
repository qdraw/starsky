using System.Collections.Immutable;
using starsky.foundation.platform.Helpers;
using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public static class CreateAnExifToolWindows
	{

		private static readonly string ImageExifToolZipWindows = "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";
		
		public static readonly ImmutableArray<byte> Bytes = Base64Helper.TryParse(ImageExifToolZipWindows).ToImmutableArray();

		public static readonly string Sha1 = "0da554d4cf5f4c15591da109ae070742ecfceb65";
	}
}
