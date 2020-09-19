using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers
{
	[TestClass]
	public class DateAssemblyTest
	{
				
		[TestMethod]
		public void DateAssemblyHealthCheck_GetBuildDate()
		{
			// this gets the one from the test assembly
			var date = DateAssembly.GetBuildDate(Assembly.GetExecutingAssembly());
			Assert.IsTrue(date.Year == 1 );
		}
		
		[TestMethod]
		public void GetBuildDate_StarskyStartup()
		{
			var date = DateAssembly.GetBuildDate(typeof(Startup).Assembly);
			Assert.IsTrue(date.Year >= 2020 );
		}
		
		[TestMethod]
		public void GetBuildDate_NonExist()
		{
			var date = DateAssembly.GetBuildDate(typeof(short).Assembly);
			Assert.IsTrue(date.Year == 1 );
		}
		
		[TestMethod]
		public void GetBuildDate_NonExist_DateAssemblyTest()
		{
			var date = DateAssembly.GetBuildDate(typeof(DateAssemblyTest).Assembly);
			Assert.IsTrue(date.Year == 1 );
		}

		[TestMethod]
		public void ParseBuildTime_WrongInput_NotContainBuild()
		{
			var date = DateAssembly.ParseBuildTime("111");
			Assert.IsTrue(date.Year == 1 );
		}
		
		[TestMethod]
		public void ParseBuildTime_WrongInput_NotContainValidDate()
		{
			var date = DateAssembly.ParseBuildTime("000+build");
			Assert.IsTrue(date.Year == 1 );
		}
	}
}
