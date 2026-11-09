using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starskytest;

namespace starskytest.Controllers;

[TestClass]
public sealed class ConnectControllerTest : DatabaseTest
{
	[TestMethod]
	public async Task GetConfig_FirstCall_GeneratesAndStoresCertificate()
	{
		var controller = new ConnectController(DbContext);

		var result = controller.GetConfig();
		await result;

		var jsonResult = result.Result as JsonResult;
		Assert.IsNotNull(jsonResult);
		Assert.IsNotNull(jsonResult.Value);

		var response = jsonResult.Value as ConnectController.ConnectConfigResponse;
		Assert.IsNotNull(response);
		Assert.IsFalse(string.IsNullOrEmpty(response.DeviceId), "DeviceId should be generated.");
	}

	[TestMethod]
	public async Task GetConfig_SecondCall_ReturnsSameCertificate()
	{
		var controller = new ConnectController(DbContext);

		// First call
		var result1 = controller.GetConfig();
		await result1;
		var jsonResult1 = result1.Result as JsonResult;
		var response1 = jsonResult1?.Value as ConnectController.ConnectConfigResponse;
		var deviceId1 = response1?.DeviceId;

		// Second call
		var result2 = controller.GetConfig();
		await result2;
		var jsonResult2 = result2.Result as JsonResult;
		var response2 = jsonResult2?.Value as ConnectController.ConnectConfigResponse;
		var deviceId2 = response2?.DeviceId;

		Assert.AreEqual(deviceId1, deviceId2, "DeviceId should remain the same between calls.");
	}

	[TestMethod]
	public async Task GetConfig_CertificateExportDoesNotThrow()
	{
		// Regression test for macOS export failure
		var controller = new ConnectController(DbContext);

		// This should not throw an exception even on macOS
		var result = controller.GetConfig();
		await result;

		var jsonResult = result.Result as JsonResult;
		Assert.IsNotNull(jsonResult, "Should return a JsonResult without throwing.");
	}
}
