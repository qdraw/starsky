using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyAdminCli.Services;
using starskycore.Models;
using starskytest.FakeMocks;

namespace starskytest.starskyAdminCli.Services
{
	
	[TestClass]
	public class ConsoleAdminTest
	{
		[TestMethod]
		public void StarskyAdminCliProgramTest_UserDoesNotExist()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"dont@mail.me"
			});
			new ConsoleAdmin(new AppSettings(), new FakeUserManagerActiveUsers(),console ).Tool();
			
			Assert.AreEqual("User dont@mail.me does not exist", console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void StarskyAdminCliProgramTest_NoInput()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				string.Empty
			});
			new ConsoleAdmin(new AppSettings(), new FakeUserManagerActiveUsers(), console ).Tool();
			
			Assert.AreEqual("No input selected", console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void StarskyAdminCliProgramTest_Removed()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"test",
				"2"
			});
			new ConsoleAdmin(new AppSettings(), new FakeUserManagerActiveUsers(), console ).Tool();
			
			Assert.AreEqual("User test is removed", console.WrittenLines.LastOrDefault());
		}
	}
}
