using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class ToneMappingTests
{
	[TestMethod]
	public void MapValue_GammaOnly_MapsLinearToDisplay()
	{
		var mapped = ToneMapping.MapValue(0.25f, 1f / 2.2f, ToneCurve.None);
		Assert.AreEqual(0.5325f, mapped, 1e-3f);
	}

	[TestMethod]
	public void MapValue_WithHableCurve_ClampsWithinDisplayRange()
	{
		var low = ToneMapping.MapValue(0f, 1f / 2.2f, ToneCurve.Hable);
		var high = ToneMapping.MapValue(16f, 1f / 2.2f, ToneCurve.Hable);

		Assert.AreEqual(0f, low, 1e-6f);
		Assert.IsTrue(high is >= 0f and <= 1f);
	}

	[TestMethod]
	public void ApplyInPlace_WithAcesCurve_ProducesFiniteRgb()
	{
		var rgb = new float[1, 1, 3];
		rgb[0, 0, 0] = 0.1f;
		rgb[0, 0, 1] = 0.5f;
		rgb[0, 0, 2] = 2f;

		ToneMapping.ApplyInPlace(rgb, 2.2f, ToneCurve.Aces);

		Assert.IsFalse(float.IsNaN(rgb[0, 0, 0]));
		Assert.IsFalse(float.IsNaN(rgb[0, 0, 1]));
		Assert.IsFalse(float.IsNaN(rgb[0, 0, 2]));
		Assert.IsTrue(rgb[0, 0, 0] is >= 0f and <= 1f);
		Assert.IsTrue(rgb[0, 0, 1] is >= 0f and <= 1f);
		Assert.IsTrue(rgb[0, 0, 2] is >= 0f and <= 1f);
	}
}

