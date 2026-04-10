using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;

namespace starskytest.Controllers;

public class FakeINominatimProxyService : INominatimProxyService
{
	public double LastLat { get; private set; }
	public double LastLon { get; private set; }
	public bool ThrowException { get; set; }

	public NominatimReverseResult? ResultToReturn { get; set; } =
		new() { DisplayName = "Test Location" };

	public Task<NominatimReverseResult> ReverseAsync(double latitude, double longitude)
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
public class GeoReverseNominatimControllerTest
{
	[TestMethod]
	public async Task GeoReverseLookup_ValidRequest_ReturnsOk()
	{
		var fakeService = new FakeINominatimProxyService();
		var controller = new GeoReverseNominatimController(fakeService);
		var result = await controller.GeoReverseLookup(52.1, 4.3) as OkObjectResult;
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(NominatimReverseResult));
		Assert.AreEqual("Test Location", ( ( NominatimReverseResult ) result.Value ).DisplayName);
		Assert.AreEqual(52.1, fakeService.LastLat);
		Assert.AreEqual(4.3, fakeService.LastLon);
	}

	[TestMethod]
	public async Task GeoReverseLookup_InvalidModelState_ReturnsBadRequest()
	{
		var controller = new GeoReverseNominatimController(new FakeINominatimProxyService());
		controller.ModelState.AddModelError("lat", "Required");
		var result = await controller.GeoReverseLookup(0, 0);
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public async Task GeoReverseLookup_ServiceThrows_ReturnsException()
	{
		var fakeService = new FakeINominatimProxyService { ThrowException = true };
		var controller = new GeoReverseNominatimController(fakeService);
		
		await Assert.ThrowsExactlyAsync<Exception>(async () => 
			await controller.GeoReverseLookup(1, 2));
	}
}
