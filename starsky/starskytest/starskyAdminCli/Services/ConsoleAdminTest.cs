using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starskyAdminCli.Services;
using starskytest.FakeMocks;

namespace starskytest.starskyAdminCli.Services
{
	[TestClass]
	public sealed class ConsoleAdminTest
	{
		[TestMethod]
		public async Task StarskyAdminCliProgramTest_UserDoesNotExist_AndCreateAccount()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"dont@mail.me", "1234567890123456"
			});

			var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console);
			await service.Tool(string.Empty,
				string.Empty);

			Assert.AreEqual("User dont@mail.me is created",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task CreateAccount_AsInput()
		{
			var console = new FakeConsoleWrapper();
			var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console);
			await service.Tool("dont@mail.me", "1234567890123456");

			Assert.AreEqual("User dont@mail.me is created",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task UserCreate_ValidationShouldFail()
		{
			var console = new FakeConsoleWrapper(new List<string> { "no_email", "false" });

			var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console);
			await service.Tool(string.Empty, string.Empty);

			Assert.AreEqual("username / password is not valid",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task UserCreate_NoInput()
		{
			var console = new FakeConsoleWrapper(new List<string> { "dont@mail.me", string.Empty });

			var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console);
			await service.Tool(string.Empty, string.Empty);

			Assert.AreEqual("No input selected",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task StarskyAdminCliProgramTest_NoInput()
		{
			var console = new FakeConsoleWrapper(new List<string> { string.Empty });
			await new ConsoleAdmin(new FakeUserManagerActiveUsers(), console).Tool(string.Empty,
				string.Empty);

			Assert.AreEqual("No input selected",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task StarskyAdminCliProgramTest_Removed()
		{
			var console = new FakeConsoleWrapper(new List<string> { "test", "2" });
			await new ConsoleAdmin(
					new FakeUserManagerActiveUsers("test", new User { Name = "t1", Id = 99 }),
					console)
				.Tool(string.Empty, string.Empty);

			Assert.AreEqual("User test is removed",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task ToggleUserAdminRole_toAdmin()
		{
			var console = new FakeConsoleWrapper(new List<string> { "test", "3" });
			await new ConsoleAdmin(
				new FakeUserManagerActiveUsers("test", new User { Name = "t1", Id = 99 }), console
			).Tool(string.Empty, string.Empty);

			Assert.AreEqual("User test has now the role Administrator",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task ToggleUserAdminRole_toUser()
		{
			var console = new FakeConsoleWrapper(new List<string> { "test", "3" });

			var userMan = new FakeUserManagerActiveUsers("test", new User { Name = "t1", Id = 99 })
			{
				Role = new Role { Code = AccountRoles.AppAccountRoles.Administrator.ToString() }
			};

			await new ConsoleAdmin(userMan, console).Tool(string.Empty, string.Empty);
			Assert.AreEqual("User test has now the role User",
				console.WrittenLines.LastOrDefault());
		}

		[TestMethod]
		public async Task ToggleUserAdminRole_toUser_invalidEnum_selected()
		{
			var console = new FakeConsoleWrapper(new List<string> { "test", "q" });

			var userMan = new FakeUserManagerActiveUsers("test", new User { Name = "t1", Id = 99 })
			{
				Role = new Role { Code = AccountRoles.AppAccountRoles.Administrator.ToString() }
			};

			await new ConsoleAdmin(userMan, console).Tool(string.Empty, string.Empty);
			Assert.AreEqual("No input selected ends now", console.WrittenLines.LastOrDefault());
		}
	}
}
