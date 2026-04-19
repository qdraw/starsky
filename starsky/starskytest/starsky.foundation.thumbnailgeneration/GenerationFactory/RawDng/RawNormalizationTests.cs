using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class RawNormalizationTests
{
	[TestMethod]
	public void NormalizeSample_ClampsAcrossRange()
	{
		const float black = 64f;
		const float white = 1024f;

		Assert.AreEqual(0f, RawNormalization.NormalizeSample(0, black, white));
		Assert.AreEqual(0f, RawNormalization.NormalizeSample(64, black, white));
		Assert.AreEqual(0.5f, RawNormalization.NormalizeSample(544, black, white), 1e-6f);
		Assert.AreEqual(1f, RawNormalization.NormalizeSample(1024, black, white));
		Assert.AreEqual(1f, RawNormalization.NormalizeSample(4095, black, white));
	}

	[TestMethod]
	public void NormalizeBayerToLinear_ConvertsAllPixels()
	{
		ushort[,] bayer =
		{
			{ 64, 544 },
			{ 1024, 2048 }
		};

		var normalized = RawNormalization.NormalizeBayerToLinear(bayer, 64f, 1024f);

		Assert.AreEqual(2, normalized.GetLength(0));
		Assert.AreEqual(2, normalized.GetLength(1));
		Assert.AreEqual(0f, normalized[0, 0], 1e-6f);
		Assert.AreEqual(0.5f, normalized[0, 1], 1e-6f);
		Assert.AreEqual(1f, normalized[1, 0], 1e-6f);
		Assert.AreEqual(1f, normalized[1, 1], 1e-6f);
	}

	[TestMethod]
	public void NormalizeSample_WithInvalidRange_ReturnsZero()
	{
		Assert.AreEqual(0f, RawNormalization.NormalizeSample(1000, 100f, 100f));
		Assert.AreEqual(0f, RawNormalization.NormalizeSample(1000, 200f, 100f));
	}
}

