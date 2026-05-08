using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class ColorMatrixTransformTests
{
	[TestMethod]
	public void BuildCameraToSrgb_WithIdentityColorMatrix_ReturnsFinite3x3()
	{
		var cameraToSrgb = ColorMatrixTransform.BuildCameraToSrgb(new[,]
		{
			{ 1f, 0f, 0f },
			{ 0f, 1f, 0f },
			{ 0f, 0f, 1f }
		}, new[,]
		{
			{ 1f, 0f, 0f },
			{ 0f, 1f, 0f },
			{ 0f, 0f, 1f }
		}, 17);

		Assert.AreEqual(3, cameraToSrgb.GetLength(0));
		Assert.AreEqual(3, cameraToSrgb.GetLength(1));
		for ( var row = 0; row < 3; row++ )
		{
			for ( var col = 0; col < 3; col++ )
			{
				Assert.IsFalse(float.IsNaN(cameraToSrgb[row, col]));
				Assert.IsFalse(float.IsInfinity(cameraToSrgb[row, col]));
			}
		}
	}

	[TestMethod]
	public void ApplyInPlace_WithIdentityMatrix_KeepsValues()
	{
		var rgb = new float[1, 1, 3];
		rgb[0, 0, 0] = 0.1f;
		rgb[0, 0, 1] = 0.2f;
		rgb[0, 0, 2] = 0.3f;

		ColorMatrixTransform.ApplyInPlace(rgb, new[,]
		{
			{ 1f, 0f, 0f },
			{ 0f, 1f, 0f },
			{ 0f, 0f, 1f }
		});

		Assert.AreEqual(0.1f, rgb[0, 0, 0], 1e-6f);
		Assert.AreEqual(0.2f, rgb[0, 0, 1], 1e-6f);
		Assert.AreEqual(0.3f, rgb[0, 0, 2], 1e-6f);
	}

	[TestMethod]
	public void BuildCameraToSrgb_WithUserFixtureColorMatrix_MapsWhiteBalancedNeutralToGray()
	{
		var asShotNeutral = new[] { 0.60204697f, 1.04931796f, 0.64599484f };
		var gains = WhiteBalance.GainsFromAsShotNeutral(asShotNeutral);
		var cameraToSrgb = ColorMatrixTransform.BuildCameraToSrgb(new[,]
		{
			{ 0.544808f, -0.174047f, -0.080399f },
			{ -0.075055f, 0.440444f, 0.011367f },
			{ -0.005801f, 0.071589f, 0.118914f }
		}, new[,]
		{
			{ 1f, 0f, 0f },
			{ 0f, 1f, 0f },
			{ 0f, 0f, 1f }
		}, 1, asShotNeutral: asShotNeutral);

		var rgb = new float[1, 1, 3];
		rgb[0, 0, 0] = asShotNeutral[0];
		rgb[0, 0, 1] = asShotNeutral[1];
		rgb[0, 0, 2] = asShotNeutral[2];

		WhiteBalance.ApplyInPlace(rgb, gains);
		ColorMatrixTransform.ApplyInPlace(rgb, cameraToSrgb);

		Assert.AreEqual(rgb[0, 0, 0], rgb[0, 0, 1], 1e-3f);
		Assert.AreEqual(rgb[0, 0, 1], rgb[0, 0, 2], 1e-3f);
		Assert.IsGreaterThan(0f, rgb[0, 0, 0]);
	}
}
