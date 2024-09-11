using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class GeoReverseLookupControllerTest
{
	[TestMethod]
	public async Task GeoReverseLookup_Index()
	{
		var controller = new GeoReverseLookupController(new FakeIGeoReverseLookup());
		var result = await controller.GeoReverseLookup(0, 0) as OkObjectResult;
		Assert.AreEqual(200, result?.StatusCode);
	}
	
	[TestMethod]
	public async Task GeoReverseLookup_ReturnsBadRequest()
	{
		// Arrange
		var controller = new GeoReverseLookupController(new FakeIGeoReverseLookup());
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		// Act
		var result = await controller.GeoReverseLookup(0, 0);

		// Assert
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
	}
}
