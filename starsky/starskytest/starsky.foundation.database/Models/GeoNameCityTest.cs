using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public class GeoNameCityTest
{
	[TestMethod]
	public void GetTimeZoneTest_DefaultToUtc()
	{
		var geoNameCity = new GeoNameCity
		{
			Name = "test", CountryCode = "NL", TimeZoneId = "Invalid"
		};
		var timeZone = geoNameCity.GetTimeZone();
		Assert.AreEqual("UTC", timeZone.Id);
	}
}
