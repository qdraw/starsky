using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starskyAdminCli.Services;
using starskytest.FakeMocks;

namespace starskytest.starskyAdminCli.Services;

[TestClass]
public sealed class ConsoleAdminTest
{
	[TestMethod]
	public async Task StarskyAdminCliProgramTest_UserDoesNotExist_AndCreateAccount()
	{
		var console =
			new FakeConsoleWrapper(["dont@mail.me", "1234567890123456"]);

		var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console,
			new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>()));
		await service.Tool(string.Empty,
			string.Empty);

		Assert.AreEqual("User dont@mail.me is created",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task CreateAccount_AsInput()
	{
		var console = new FakeConsoleWrapper();
		var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console,
			new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>()));
		await service.Tool("dont@mail.me", "1234567890123456");

		Assert.AreEqual("User dont@mail.me is created",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task UserCreate_ValidationShouldFail()
	{
		var console = new FakeConsoleWrapper(["no_email", "false"]);

		var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console,
			new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>()));
		await service.Tool(string.Empty, string.Empty);

		Assert.AreEqual("username / password is not valid",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task UserCreate_NoInput()
	{
		var console = new FakeConsoleWrapper(["dont@mail.me", string.Empty]);

		var service = new ConsoleAdmin(new FakeUserManagerActiveUsers(), console,
			new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>()));
		await service.Tool(string.Empty, string.Empty);

		Assert.AreEqual("No input selected",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task StarskyAdminCliProgramTest_NoInput()
	{
		var console = new FakeConsoleWrapper([string.Empty]);
		await new ConsoleAdmin(new FakeUserManagerActiveUsers(), console, new FakeIHttpClientHelper(
			new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>())).Tool(string.Empty,
			string.Empty);

		Assert.AreEqual("No input selected",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task StarskyAdminCliProgramTest_Removed()
	{
		var console = new FakeConsoleWrapper(["test", "2"]);
		await new ConsoleAdmin(
				new FakeUserManagerActiveUsers("test", new User { Name = "t1", Id = 99 }),
				console, new FakeIHttpClientHelper(new FakeIStorage(),
					new Dictionary<string, KeyValuePair<bool, string>>()))
			.Tool(string.Empty, string.Empty);

		Assert.AreEqual("User test is removed",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task ToggleUserAdminRole_toAdmin()
	{
		var console = new FakeConsoleWrapper(["test", "3", "n"]);
		var service = new ConsoleAdmin(
			new FakeUserManagerActiveUsers("test",
				new User
				{
					Name = "t1",
					Id = 99,
					Credentials = new List<Credential> { new() { Identifier = "test" } }
				}), console, new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>())
		);

		await service.Tool(string.Empty, string.Empty);

		Assert.Contains("User test has now the role Administrator", console.WrittenLines);
	}

	[TestMethod]
	public async Task ToggleUserAdminRole_toUser()
	{
		var console = new FakeConsoleWrapper(["test", "3"]);

		var userMan =
			new FakeUserManagerActiveUsers("test",
				new User
				{
					Name = "t1",
					Id = 99,
					Credentials = new List<Credential> { new() { Identifier = "test" } }
				})
			{
				Role = new Role { Code = AccountRoles.AppAccountRoles.Administrator.ToString() }
			};

		await new ConsoleAdmin(userMan, console, new FakeIHttpClientHelper(new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>())).Tool(string.Empty, string.Empty);
		Assert.AreEqual("User test has now the role User",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task ToggleUserAdminRole_toAdmin_AndPlatformAdmin()
	{
		var console = new FakeConsoleWrapper(["test", "3", "y"]);
		var user = new User
		{
			Name = "t1",
			Id = 99,
			Credentials = new List<Credential> { new() { Identifier = "test" } }
		};

		await new ConsoleAdmin(new FakeUserManagerActiveUsers("test", user), console,
			new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>())).Tool(string.Empty,
			string.Empty);

		Assert.IsTrue(user.IsPlatformAdmin);
		Assert.AreEqual("User test is now platform admin", console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task ToggleUserAdminRole_toUser_RemovesPlatformAdmin()
	{
		var console = new FakeConsoleWrapper(["test", "3"]);
		var user = new User
		{
			Name = "t1",
			Id = 99,
			IsPlatformAdmin = true,
			Credentials = new List<Credential> { new() { Identifier = "test" } }
		};
		var userMan = new FakeUserManagerActiveUsers("test", user)
		{
			Role = new Role { Code = AccountRoles.AppAccountRoles.Administrator.ToString() }
		};

		await new ConsoleAdmin(userMan, console, new FakeIHttpClientHelper(new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>())).Tool(string.Empty, string.Empty);

		Assert.IsFalse(user.IsPlatformAdmin);
		Assert.AreEqual("User test is no longer platform admin",
			console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task ToggleUserAdminRole_toUser_invalidEnum_selected()
	{
		var console = new FakeConsoleWrapper(["test", "q"]);

		var userMan = new FakeUserManagerActiveUsers("test", new User { Name = "t1", Id = 99 })
		{
			Role = new Role { Code = AccountRoles.AppAccountRoles.Administrator.ToString() }
		};

		await new ConsoleAdmin(userMan, console, new FakeIHttpClientHelper(new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>())).Tool(string.Empty, string.Empty);
		Assert.AreEqual("No input selected ends now", console.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public async Task DropboxSetup()
	{
		var console = new FakeConsoleWrapper([
			"test",
			"4",
			"test-app-key", // Dropbox App Key
			"test-app-secret", // Dropbox App Secret
			"", // Tenant id: empty is the default tenant
			"test-access-code", // Access code
			""
		]);
		var httpClientHelper = new FakeIHttpClientHelper(
			new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://api.dropbox.com/oauth2/token",
					new KeyValuePair<bool, string>(true, "{\"refresh_token\":\"refresh-token\"}")
				}
			}
		);

		var service = new ConsoleAdmin(
			new FakeUserManagerActiveUsers("test",
				new User
				{
					Name = "t1",
					Id = 99,
					Credentials = new List<Credential> { new() { Identifier = "test" } }
				}),
			console,
			httpClientHelper
		);

		await service.Tool(string.Empty, string.Empty);

		Assert.IsTrue(console.WrittenLines.Exists(x => x.Contains("Dropbox Setup:")));
		Assert.IsTrue(console.WrittenLines.Exists(x =>
			x.Contains("Go to: https://www.dropbox.com/developers/apps/create")));
		Assert.IsTrue(console.WrittenLines.Exists(x =>
			x.Contains("Merge this with an existing appsettings.json:")));
		Assert.IsTrue(console.WrittenLines.Exists(x => x.Contains("refresh-token")));
		Assert.IsTrue(console.WrittenLines.Exists(x => x.Contains("\"tenantId\": \"default\"")));
	}

	[TestMethod]
	public async Task DropboxSetup_TenantId_InvalidThenValid()
	{
		var console = new FakeConsoleWrapper([
			"test", "4", "test-app-key", "test-app-secret",
			"Not Valid!", // asked again
			"acme", "test-access-code", ""
		]);
		var httpClientHelper = new FakeIHttpClientHelper(
			new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://api.dropbox.com/oauth2/token",
					new KeyValuePair<bool, string>(true, "{\"refresh_token\":\"refresh-token\"}")
				}
			});
		var service = new ConsoleAdmin(
			new FakeUserManagerActiveUsers("test",
				new User
				{
					Name = "t1",
					Id = 99,
					Credentials = new List<Credential> { new() { Identifier = "test" } }
				}), console, httpClientHelper);

		await service.Tool(string.Empty, string.Empty);

		Assert.IsTrue(console.WrittenLines.Exists(x => x.Contains("is not a valid tenant id")));
		Assert.IsTrue(console.WrittenLines.Exists(x => x.Contains("\"tenantId\": \"acme\"")));
	}
}
