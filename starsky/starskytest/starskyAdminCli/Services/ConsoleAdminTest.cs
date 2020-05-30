using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
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

		[TestMethod]
		public void ToggleUserAdminRole_toAdmin()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"test",
				"3"
			});
			new ConsoleAdmin(new AppSettings(), new FakeUserManagerActiveUsers(),console ).Tool();
			Assert.AreEqual("User test has now the role Administrator", console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void ToggleUserAdminRole_toUser()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"test",
				"3"
			});
			
			var userMan = new FakeUserManagerActiveUsers
			{
				Role = new Role {Code = AccountRoles.AppAccountRoles.Administrator.ToString()}
			};
			
			new ConsoleAdmin(new AppSettings(), userMan,console ).Tool();
			Assert.AreEqual("User test has now the role User", console.WrittenLines.LastOrDefault());
		}
	}
}
