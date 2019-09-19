using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.Controllers;

namespace starskytest.Controllers
{
	[TestClass]
	public class HealthControllerTest
	{
		[TestMethod]
		public void HealthControllerRenderCheck()
		{
			new HealthController().Health();
		}
		

		[TestMethod]
		public void HealthControllerTest_GetBuildDate()
		{
			// this gets the one from the test assembly
			var date = HealthController.GetBuildDate(Assembly.GetExecutingAssembly());
			Assert.IsTrue(date.Year == 1 );
		}
		
		[TestMethod]
		public void GetBuildDate_Starsky()
		{
			var date = HealthController.GetBuildDate(typeof(starsky.Startup).Assembly);
			Assert.IsTrue(date.Year >= 2019 );
		}
		
		[TestMethod]
		public void GetBuildDate_NonExist()
		{
			var date = HealthController.GetBuildDate(typeof(short).Assembly);
			Assert.IsTrue(date.Year == 1 );
		}


	}
}
