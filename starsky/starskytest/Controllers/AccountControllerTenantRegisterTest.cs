using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Models.Account;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

/// <summary>
///     Registering must never join a tenant by itself: anybody who is logged in (or any visitor
///     when registering is open) could otherwise add an account to any tenant
/// </summary>
[TestClass]
public sealed class AccountControllerTenantRegisterTest
{
	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
	}

	private static (AccountController Controller, FakeUserManagerActiveUsers Manager) Create(
		string tenantId, bool isOpen, params Claim[] claims)
	{
		var manager = new FakeUserManagerActiveUsers();
		var tenant = new TenantContext();
		tenant.SetTenant(tenantId, false);
		var controller = new AccountController(manager,
			new AppSettings { IsAccountRegisterOpen = isOpen }, new FakeAntiforgery(),
			new FakeSelectorStorage(), tenant);
		controller.ControllerContext.HttpContext = new DefaultHttpContext
		{
			User = claims.Length == 0
				? new ClaimsPrincipal(new ClaimsIdentity())
				: new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
		};
		return ( controller, manager );
	}

	private static RegisterViewModel NewAccount()
	{
		return new RegisterViewModel
		{
			Email = "new@example.test", Password = "password-1234",
			ConfirmPassword = "password-1234", Name = "New"
		};
	}

	private static Claim Administrator()
	{
		return new Claim(ClaimTypes.Role, "Administrator");
	}

	private static Claim PlainUser()
	{
		return new Claim(ClaimTypes.Role, "User");
	}

	private static Claim PlatformAdmin()
	{
		return new Claim(TenantMembershipMiddleware.PlatformAdminClaim, "true");
	}

	private static async Task<string?> Register(AccountController controller)
	{
		return ( await controller.Register(NewAccount()) as JsonResult )?.Value as string;
	}

	[TestMethod]
	public async Task Closed_LoggedInPlainUser_Refused()
	{
		var (controller, manager) = Create("acme", false, PlainUser());

		Assert.AreEqual("Account Register page is closed", await Register(controller));
		Assert.IsNull(manager.LastJoinCurrentTenant);
	}

	[TestMethod]
	public async Task Closed_Anonymous_Refused()
	{
		var (controller, manager) = Create("default", false);

		Assert.AreEqual("Account Register page is closed", await Register(controller));
		Assert.IsNull(manager.LastJoinCurrentTenant);
	}

	[TestMethod]
	public async Task Closed_AdministratorOfTheTenant_CreatesAnAccountThatJoins()
	{
		var (controller, manager) = Create("acme", false, Administrator());

		Assert.AreEqual("Account Created", await Register(controller));
		Assert.AreEqual(true, manager.LastJoinCurrentTenant);
	}

	[TestMethod]
	public async Task Closed_PlatformAdministratorWithoutRoleInTheTenant_CreatesAccountWithoutMembership()
	{
		var (controller, manager) = Create("acme", false, PlatformAdmin());

		Assert.AreEqual("Account Created", await Register(controller));
		Assert.AreEqual(false, manager.LastJoinCurrentTenant);
	}

	[TestMethod]
	public async Task Open_AnonymousOnAnotherTenant_CreatesAccountWithoutMembership()
	{
		var (controller, manager) = Create("acme", true);

		Assert.AreEqual("Account Created", await Register(controller));
		Assert.AreEqual(false, manager.LastJoinCurrentTenant);
	}

	[TestMethod]
	public async Task Open_LoggedInUserOfAnotherTenant_CreatesAccountWithoutMembership()
	{
		var (controller, manager) = Create("acme", true, PlainUser());

		Assert.AreEqual("Account Created", await Register(controller));
		Assert.AreEqual(false, manager.LastJoinCurrentTenant);
	}

	[TestMethod]
	public async Task Open_DefaultTenant_WorksAsBefore()
	{
		var (controller, manager) = Create("default", true);

		Assert.AreEqual("Account Created", await Register(controller));
		Assert.AreEqual(true, manager.LastJoinCurrentTenant);
	}
}
