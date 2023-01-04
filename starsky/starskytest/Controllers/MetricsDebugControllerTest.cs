using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskycore.ViewModels;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class MetricsDebugControllerTest
{
	[TestMethod]
	public void MetricsDebugController_Index_test()
	{
		var controller = new MetricsDebugController(new FakeICpuUsageListenerBackgroundService(1d));
		var jsonResult = controller.Index() as JsonResult;
		var resultValue = jsonResult!.Value as MetricsDebugViewModel;

		Assert.IsNotNull(jsonResult);
		Assert.AreEqual( 1d, resultValue?.CpuUsageMean);
	}
}
