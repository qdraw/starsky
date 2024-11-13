using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public class RotationModelTest
{
	[TestMethod]
	[DataRow(RotationModel.Rotation.DoNotChange, 0)]
	[DataRow(RotationModel.Rotation.Horizontal, 0)]
	[DataRow(RotationModel.Rotation.Rotate90Cw, 90)]
	[DataRow(RotationModel.Rotation.Rotate180, 180)]
	[DataRow(RotationModel.Rotation.Rotate270Cw, 270)]
	public void RotateEnumToDegrees(RotationModel.Rotation rotation, int expected)
	{
		var result = rotation.ToDegrees();
		Assert.AreEqual(expected, result, 0.00001);
	}
}
