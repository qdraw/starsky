using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starskyAdminCli.Services;
using starskytest.FakeMocks;

namespace starskytest.starskyAdminCli.Services
{
	
	[TestClass]
	public class ConsoleAdminTest
	{
		[TestMethod]
		public void StarskyAdminCliProgramTest_UserDoesNotExist_AndCreateAccount()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"dont@mail.me",
				"1234567890123456"
			});
			new ConsoleAdmin(new FakeUserManagerActiveUsers(),console ).Tool(string.Empty, string.Empty);

			Assert.AreEqual("User dont@mail.me is created", 
				console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void CreateAccount_AsInput()
		{
			var console = new FakeConsoleWrapper();
			new ConsoleAdmin(new FakeUserManagerActiveUsers(),console ).Tool("dont@mail.me", "1234567890123456");

			Assert.AreEqual("User dont@mail.me is created", 
				console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void UserCreate_ValidationShouldFail()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"no_email",
				"false"
			});
			new ConsoleAdmin(new FakeUserManagerActiveUsers(),console ).Tool(string.Empty, string.Empty);

			foreach ( var line in console.WrittenLines )
			{
				Console.WriteLine(line);
			}
			
			Assert.AreEqual("username / password is not valid", 
				console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void StarskyAdminCliProgramTest_NoInput()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				string.Empty
			});
			new ConsoleAdmin(new FakeUserManagerActiveUsers(), console ).Tool(string.Empty,string.Empty);
			
			Assert.AreEqual("No input selected", 
				console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void StarskyAdminCliProgramTest_Removed()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"test",
				"2"
			});
			new ConsoleAdmin(new FakeUserManagerActiveUsers(), console )
				.Tool(string.Empty,string.Empty);
			
			Assert.AreEqual("User test is removed", 
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public void ToggleUserAdminRole_toAdmin()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"test",
				"3"
			});
			new ConsoleAdmin( new FakeUserManagerActiveUsers(),console ).Tool(string.Empty,string.Empty);
			Assert.AreEqual("User test has now the role Administrator", 
				console.WrittenLines.LastOrDefault());
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
			
			new ConsoleAdmin( userMan,console ).Tool(string.Empty,string.Empty);
			Assert.AreEqual("User test has now the role User", console.WrittenLines.LastOrDefault());
		}
	}
}
