using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public class ImageRotationTest
{
	[TestMethod]
	public void RotateEnumToDegrees_Horizontal()
	{
		var result = ImageRotation.Rotation.Horizontal.ToDegrees();
		Assert.AreEqual(0, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_Default()
	{
		var result = ImageRotation.Rotation.DoNotChange.ToDegrees();
		Assert.AreEqual(0, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_180()
	{
		var result = ImageRotation.Rotation.Rotate180.ToDegrees();
		Assert.AreEqual(180, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_90()
	{
		var result = ImageRotation.Rotation.Rotate90Cw.ToDegrees();
		Assert.AreEqual(90, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_270()
	{
		var result = ImageRotation.Rotation.Rotate270Cw.ToDegrees();
		Assert.AreEqual(270, result, 0.00001);
	}
}
