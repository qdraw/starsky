using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class ColorMatrixTransformTests
{
	[TestMethod]
	public void BuildCameraToSrgb_WithIdentityColorMatrix_ReturnsXyzToSrgb()
	{
		var cameraToSrgb = ColorMatrixTransform.BuildCameraToSrgb(new[,]
		{
			{ 1f, 0f, 0f },
			{ 0f, 1f, 0f },
			{ 0f, 0f, 1f }
		});

		Assert.AreEqual(3.2404542f, cameraToSrgb[0, 0], 1e-6f);
		Assert.AreEqual(-1.5371385f, cameraToSrgb[0, 1], 1e-6f);
		Assert.AreEqual(1.0572252f, cameraToSrgb[2, 2], 1e-6f);
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

