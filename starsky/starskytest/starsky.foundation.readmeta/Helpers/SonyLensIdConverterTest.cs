using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.Helpers;

namespace starskytest.starsky.foundation.readmeta.Helpers;

[TestClass]
public class SonyLensIdConverterTest
{
	[TestMethod]
	public void IsGenericEMountTMountOtherLens()
	{
		var generalLens = SonyLensIdConverter.IsGenericEMountTMountOtherLens("65535");
		Assert.IsTrue(generalLens);
	}
	
	[TestMethod]
	[DataRow("0", "Minolta AF 28-85mm F3.5-4.5 New")]
	[DataRow("55", "Sony DT 18-55mm F3.5-5.6 SAM (SAL1855) or SAM II")]
	[DataRow("61184", "Canon EF Adapter")]
	[DataRow("65535", "E-Mount, T-Mount, Other Lens or no lens")]
	[DataRow("NOT_FOUND_ID", null)]
	public void GetById(string id, string? expected)
	{
		var generalLens = SonyLensIdConverter.GetById(id);
		Assert.AreEqual(expected,generalLens);
	}
}
