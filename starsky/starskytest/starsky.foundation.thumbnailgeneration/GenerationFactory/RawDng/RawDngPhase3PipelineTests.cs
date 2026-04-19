using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class RawDngPhase3PipelineTests
{
	[TestMethod]
	public void Run_ProducesNormalizedAndLinearRgb_WithWhiteBalanceApplied()
	{
		var raw = new DngRawImage
		{
			Width = 2,
			Height = 2,
			BitsPerSample = 16,
			BlackLevel = new[] { 0f, 0f, 0f, 0f },
			WhiteLevel = new[] { 1024f, 1024f, 1024f, 1024f },
			AsShotNeutral = new[] { 2f, 1f, 4f },
			ColorMatrix1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CfaPattern = new byte[] { 0, 1, 1, 2 },
			ForwardMatrix1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CameraCalibration1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CameraCalibration2 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			Bayer = new ushort[,]
			{
				{ 0, 512 },
				{ 1024, 1024 }
			}
		};

		var state = RawDngPhase3Pipeline.Run(raw);

		Assert.IsNotNull(state.NormalizedBayer);
		Assert.IsNotNull(state.LinearRgb);
		Assert.IsNotNull(state.DisplayRgb);
		Assert.IsNotNull(state.WhiteBalanceGains);
		Assert.IsNotNull(state.CameraToSrgbMatrix);
		Assert.AreEqual(0f, state.NormalizedBayer[0, 0], 1e-6f);
		Assert.AreEqual(0.5f, state.NormalizedBayer[0, 1], 1e-6f);
		Assert.AreEqual(1f, state.NormalizedBayer[1, 0], 1e-6f);
		Assert.AreEqual(1f, state.NormalizedBayer[1, 1], 1e-6f);

		Assert.AreEqual(0.5f, state.WhiteBalanceGains[0], 1e-6f);
		Assert.AreEqual(1f, state.WhiteBalanceGains[1], 1e-6f);
		Assert.AreEqual(0.25f, state.WhiteBalanceGains[2], 1e-6f);

		// Color matrix mixes channels, so site-local assumptions no longer hold.
		Assert.IsFalse(float.IsNaN(state.LinearRgb[0, 0, 0]));
		Assert.IsFalse(float.IsNaN(state.LinearRgb[1, 1, 2]));
		Assert.AreNotEqual(0.25f, state.LinearRgb[1, 1, 2], 1e-6f);

		Assert.IsTrue(state.DisplayRgb[0, 0, 0] is >= 0f and <= 1f);
		Assert.IsTrue(state.DisplayRgb[1, 1, 2] is >= 0f and <= 1f);
	}
}





