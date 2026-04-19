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
		});

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
}

