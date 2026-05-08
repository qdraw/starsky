using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class BilinearDemosaicTests
{
	[TestMethod]
	public void Demosaic_WithUniformBayer_ReturnsUniformRgb()
	{
		var bayer = new float[,]
		{
			{ 0.25f, 0.25f, 0.25f, 0.25f },
			{ 0.25f, 0.25f, 0.25f, 0.25f },
			{ 0.25f, 0.25f, 0.25f, 0.25f },
			{ 0.25f, 0.25f, 0.25f, 0.25f }
		};
		var rgb = BilinearDemosaic.Demosaic(bayer, new byte[] { 0, 1, 1, 2 });

		for ( var y = 0; y < 4; y++ )
		{
			for ( var x = 0; x < 4; x++ )
			{
				Assert.AreEqual(0.25f, rgb[y, x, 0], 1e-6f);
				Assert.AreEqual(0.25f, rgb[y, x, 1], 1e-6f);
				Assert.AreEqual(0.25f, rgb[y, x, 2], 1e-6f);
			}
		}
	}

	[TestMethod]
	public void Demosaic_WithRggbPattern_PreservesKnownSiteValues()
	{
		var bayer = new float[,]
		{
			{ 1f, 0.2f },
			{ 0.3f, 0.6f }
		};
		var rgb = BilinearDemosaic.Demosaic(bayer, new byte[] { 0, 1, 1, 2 });

		Assert.AreEqual(1f, rgb[0, 0, 0], 1e-6f); // R site
		Assert.AreEqual(0.2f, rgb[0, 1, 1], 1e-6f); // G site
		Assert.AreEqual(0.3f, rgb[1, 0, 1], 1e-6f); // G site
		Assert.AreEqual(0.6f, rgb[1, 1, 2], 1e-6f); // B site
	}
}

