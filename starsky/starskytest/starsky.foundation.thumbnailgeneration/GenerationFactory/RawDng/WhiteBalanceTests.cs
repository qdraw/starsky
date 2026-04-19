using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class WhiteBalanceTests
{
	[TestMethod]
	public void GainsFromAsShotNeutral_ComputesInverseAndNormalizesByGreen()
	{
		var gains = WhiteBalance.GainsFromAsShotNeutral([2f, 1f, 4f]);

		Assert.AreEqual(0.5f, gains[0], 1e-6f);
		Assert.AreEqual(1f, gains[1], 1e-6f);
		Assert.AreEqual(0.25f, gains[2], 1e-6f);
	}

	[TestMethod]
	public void ApplyInPlace_ScalesRgbChannels()
	{
		var rgb = new float[1, 1, 3];
		rgb[0, 0, 0] = 0.8f;
		rgb[0, 0, 1] = 0.4f;
		rgb[0, 0, 2] = 0.2f;

		WhiteBalance.ApplyInPlace(rgb, [0.5f, 1f, 2f]);

		Assert.AreEqual(0.4f, rgb[0, 0, 0], 1e-6f);
		Assert.AreEqual(0.4f, rgb[0, 0, 1], 1e-6f);
		Assert.AreEqual(0.4f, rgb[0, 0, 2], 1e-6f);
	}
}

