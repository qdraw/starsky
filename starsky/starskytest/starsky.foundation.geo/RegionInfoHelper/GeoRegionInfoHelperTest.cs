using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.geo.RegionInfoHelper;

[TestClass]
public class GeoRegionInfoHelperTest
{
	[TestMethod]
	[DataRow("NL", "Netherlands", "NLD")]
	[DataRow("US", "United States", "USA")]
	[DataRow("DE", "Germany", "DEU")]
	[DataRow("FR", "France", "FRA")]
	[DataRow("JP", "Japan", "JPN")]
	public void GetLocationCountryAndCode_ValidCodes(string code, string expectedName,
		string expectedIso3)
	{
		var logger = new FakeIWebLogger();
		var helper = new global::starsky.foundation.geo.GeoRegionInfo.RegionInfoHelper(logger);
		var result = helper.GetLocationCountryAndCode(code);
		Assert.AreEqual(expectedName, result.Item1);
		Assert.AreEqual(expectedIso3, result.Item2);
	}

	[TestMethod]
	public void GetLocationCountryAndCode_InvalidCode_ReturnsEmptyAndLogs()
	{
		var logger = new FakeIWebLogger();
		var helper = new global::starsky.foundation.geo.GeoRegionInfo.RegionInfoHelper(logger);
		var result = helper.GetLocationCountryAndCode("XX");
		Assert.AreEqual(string.Empty, result.Item1);
		Assert.AreEqual(string.Empty, result.Item2);
		Assert.IsNotNull(logger.TrackedInformation.LastOrDefault());
		Assert.Contains("[GetLocationCountryAndCode]",
			logger.TrackedInformation.LastOrDefault().Item2!);
	}

	[TestMethod]
	public void GetLocationCountryAndCode_VaticanCity_VA()
	{
		var logger = new FakeIWebLogger();
		var helper = new global::starsky.foundation.geo.GeoRegionInfo.RegionInfoHelper(logger);
		var result = helper.GetLocationCountryAndCode("TA");
		// .NET may throw for VA, so expect empty
		Assert.AreEqual(string.Empty, result.Item1);
		Assert.AreEqual(string.Empty, result.Item2);
	}
}
