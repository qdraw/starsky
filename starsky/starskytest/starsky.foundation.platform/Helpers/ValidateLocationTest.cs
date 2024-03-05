using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class ValidateLocationTest
{
	[TestMethod]
	public void ZeroValue()
	{
		var result = ValidateLocation.ValidateLatitudeLongitude(0, 0);
		Assert.IsTrue(result);
	}
	
	[TestMethod]
	public void WrongLat()
	{
		var result = ValidateLocation.ValidateLatitudeLongitude(5648994586, 0);
		Assert.IsFalse(result);
	}
	
		
	[TestMethod]
	public void WrongLong()
	{
		var result = ValidateLocation.ValidateLatitudeLongitude(51, 47844444);
		Assert.IsFalse(result);
	}
	
	[TestMethod]
	public void MoreThan6decimals()
	{
		var result = ValidateLocation.ValidateLatitudeLongitude(51.37887345983459834539458, 15);
		Assert.IsTrue(result);
	}
}
