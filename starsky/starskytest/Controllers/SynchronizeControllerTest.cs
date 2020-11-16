using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class SynchronizeControllerTest
	{
		[TestMethod]
		public async Task Index_Result200()
		{
			var actionResult = await new SynchronizeController(new FakeISynchronize()).Index("/") as OkObjectResult;
			
			Assert.AreEqual(200, actionResult.StatusCode);
		}
	}
}
