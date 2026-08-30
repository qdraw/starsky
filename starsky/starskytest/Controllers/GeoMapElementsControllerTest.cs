using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;

namespace starskytest.Controllers;

public class FakeIOsmMapElementsService : IOsmMapElementsService
{
	public double LastLat { get; private set; }
	public double LastLon { get; private set; }
	public bool ThrowException { get; set; }

	public OsmMapElementsResult? ResultToReturn { get; set; } =
		new() { NearbyObjects = [], EnclosingObjects = [] };

	public Task<OsmMapElementsResult> LookupAsync(double latitude, double longitude)
	{
		LastLat = latitude;
		LastLon = longitude;
		if ( ThrowException )
		{
			throw new Exception("Service error");
		}

		return ResultToReturn == null
			? throw new NullReferenceException()
			: Task.FromResult(ResultToReturn);
	}
}

[TestClass]
public class GeoMapElementsControllerTest
{
	[TestMethod]
	public async Task GeoMapElements_ValidRequest_ReturnsOk()
	{
		var fakeService = new FakeIOsmMapElementsService();
		var controller = new GeoMapElementsController(fakeService);
		var result = await controller.GeoMapElements(52.1, 4.3) as OkObjectResult;
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(OsmMapElementsResult));
		Assert.AreEqual(52.1, fakeService.LastLat);
		Assert.AreEqual(4.3, fakeService.LastLon);
	}

	[TestMethod]
	public async Task GeoMapElements_InvalidModelState_ReturnsBadRequest()
	{
		var controller = new GeoMapElementsController(new FakeIOsmMapElementsService());
		controller.ModelState.AddModelError("lat", "Required");
		var result = await controller.GeoMapElements(0, 0);
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public async Task GeoMapElements_ServiceThrows_ReturnsException()
	{
		var fakeService = new FakeIOsmMapElementsService { ThrowException = true };
		var controller = new GeoMapElementsController(fakeService);

		await Assert.ThrowsExactlyAsync<Exception>(async () =>
			await controller.GeoMapElements(1, 2));
	}
}