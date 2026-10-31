using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Tenancy;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

[TestClass]
public sealed class TenantLastUsedTest
{
	private static TenantListItem Item(string id)
	{
		return new TenantListItem(id, id, "User");
	}

	[TestMethod]
	public void ResolveLanding_LastUsedWhenStillMember()
	{
		Assert.AreEqual("beta",
			TenantMembershipService.ResolveLanding([Item("default"), Item("beta")], "beta"));
	}

	[TestMethod]
	public void ResolveLanding_LastUsedNoLongerMember_FallsBack()
	{
		// several memberships: the user chooses
		Assert.IsNull(TenantMembershipService.ResolveLanding(
			[Item("default"), Item("acme")], "gone"));
		// one membership: straight in
		Assert.AreEqual("acme",
			TenantMembershipService.ResolveLanding([Item("acme")], "gone"));
	}

	[TestMethod]
	public void ResolveLanding_OneMembership_StraightIn()
	{
		Assert.AreEqual("default",
			TenantMembershipService.ResolveLanding([Item("default")], null));
	}

	[TestMethod]
	public void ResolveLanding_SeveralWithoutLastUsed_Picker_NoneIsNoAccess()
	{
		Assert.IsNull(TenantMembershipService.ResolveLanding(
			[Item("default"), Item("acme")], null));
		Assert.IsNull(TenantMembershipService.ResolveLanding(new List<TenantListItem>(), null));
		Assert.IsNull(TenantMembershipService.ResolveLanding(new List<TenantListItem>(), "beta"));
	}

	[TestMethod]
	public async Task SetLastUsed_OnlyForAMembership_AndIsReadBack()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");
		var member = ( await setup.UserManagerFor(context, "default")
			.SignUpAsync("m", "email", "m@x.nl", "password-1234") ).User!.Id;
		await setup.AddActiveTenant("beta");
		var service = new TenantMembershipService(context);
		await service.AddMemberAsync("beta", "m@x.nl", "User");

		Assert.IsNull(await service.GetLastUsedAsync(member));
		Assert.AreEqual(MembershipResult.NotAMember, await service.SetLastUsedAsync(member, "gamma"));
		Assert.IsNull(await service.GetLastUsedAsync(member));

		Assert.AreEqual(MembershipResult.Ok, await service.SetLastUsedAsync(member, "beta"));
		Assert.AreEqual("beta", await service.GetLastUsedAsync(member));
		Assert.AreEqual(MembershipResult.Ok, await service.SetLastUsedAsync(member, "beta"));
		// an unknown user has no membership
		Assert.AreEqual(MembershipResult.NotAMember, await service.SetLastUsedAsync(999, "default"));
	}

	private static TenantsController Controller(TenancyTestSetup setup,
		ApplicationDbContext context, int userId, string tenantId)
	{
		var tenant = new TenantContext();
		tenant.SetTenant(tenantId);
		TenantAmbient.Reset();
		var controller = new TenantsController(new TenantMembershipService(context),
			new TenantAdminService(context, setup.Registry, setup.SettingsProvider,
				new starskytest.FakeMocks.FakeIWebLogger()),
			setup.UserManagerFor(context, tenantId), tenant);
		controller.ControllerContext.HttpContext = new DefaultHttpContext
		{
			User = new ClaimsPrincipal(new ClaimsIdentity(
				[new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"))
		};
		return controller;
	}

	[TestMethod]
	public async Task Controller_Mine_LastUsedAndLanding()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");
		var manager = setup.UserManagerFor(context, "default");
		var owner = ( await manager.SignUpAsync("o", "email", "o@x.nl", "password-1234") ).User!.Id;
		await setup.AddActiveTenant("beta");
		await new TenantMembershipService(context).AddMemberAsync("beta", "o@x.nl", "User");
		var controller = Controller(setup, context, owner, "default");

		var first = ( await controller.Mine() as JsonResult )?.Value as CurrentTenantResponse;
		Assert.AreEqual("default", first?.Current);
		Assert.HasCount(2, first!.Tenants);
		Assert.IsNull(first.LastUsed);
		Assert.IsNull(first.Landing, "two memberships and no last used: the user chooses");

		var result = await controller.LastUsed(new SetLastTenantRequest("beta"));
		Assert.IsInstanceOfType<OkResult>(result);

		var second = ( await controller.Mine() as JsonResult )?.Value as CurrentTenantResponse;
		Assert.AreEqual("beta", second?.LastUsed);
		Assert.AreEqual("beta", second?.Landing);
	}

	[TestMethod]
	public async Task Controller_LastUsed_NotAMember_NotFound()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");
		var owner = ( await setup.UserManagerFor(context, "default")
			.SignUpAsync("o", "email", "o@x.nl", "password-1234") ).User!.Id;
		await setup.AddActiveTenant("beta");
		var controller = Controller(setup, context, owner, "default");

		var result = await controller.LastUsed(new SetLastTenantRequest("beta"));

		Assert.IsInstanceOfType<NotFoundObjectResult>(result);
	}

	[TestMethod]
	public async Task Controller_MemberEndpoints_MapResults()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");
		var manager = setup.UserManagerFor(context, "default");
		var owner = ( await manager.SignUpAsync("o", "email", "o@x.nl", "password-1234") ).User!.Id;
		var other = ( await manager.SignUpAsync("u", "email", "u@x.nl", "password-1234") ).User!.Id;
		var controller = Controller(setup, context, owner, "default");

		Assert.IsInstanceOfType<NotFoundObjectResult>(
			await controller.AddMember(new AddMemberRequest("nobody@x.nl", "User")));
		Assert.IsInstanceOfType<BadRequestObjectResult>(
			await controller.AddMember(new AddMemberRequest("u@x.nl", "Root")));
		Assert.IsInstanceOfType<ConflictObjectResult>(
			await controller.AddMember(new AddMemberRequest("u@x.nl", "User")));
		Assert.IsInstanceOfType<ConflictObjectResult>(await controller.RemoveMember(owner));
		Assert.IsInstanceOfType<OkResult>(await controller.SetRole(other, new SetRoleRequest("Administrator")));
		Assert.IsInstanceOfType<OkResult>(await controller.RemoveMember(owner));
		var members = ( await controller.Members() as JsonResult )?.Value as List<TenantMemberItem>;
		Assert.HasCount(1, members!);
	}

	[TestMethod]
	public async Task Controller_TenantLifecycle_MapResults()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");
		var owner = ( await setup.UserManagerFor(context, "default")
			.SignUpAsync("o", "email", "o@x.nl", "password-1234") ).User!.Id;
		var controller = Controller(setup, context, owner, "default");

		Assert.IsInstanceOfType<BadRequestObjectResult>(
			await controller.Create(new CreateTenantRequest("../x", "x")));
		Assert.IsInstanceOfType<OkResult>(
			await controller.Create(new CreateTenantRequest("acme", "Acme")));
		Assert.IsInstanceOfType<ConflictObjectResult>(
			await controller.Create(new CreateTenantRequest("acme", "Acme")));
		Assert.IsInstanceOfType<OkResult>(await controller.Disable("acme"));
		Assert.IsInstanceOfType<OkResult>(await controller.Enable("acme"));
		Assert.IsInstanceOfType<NotFoundObjectResult>(await controller.Disable("nope"));
		Assert.IsInstanceOfType<BadRequestObjectResult>(await controller.Delete("default"));
		Assert.IsInstanceOfType<OkResult>(await controller.Delete("acme"));
		// purge: only with the id as confirmation, and only after delete
		Assert.IsInstanceOfType<BadRequestObjectResult>(await controller.Purge("acme", null));
		Assert.IsInstanceOfType<BadRequestObjectResult>(await controller.Purge("acme", "other"));
		Assert.IsInstanceOfType<OkResult>(await controller.Purge("acme", "acme"));
		Assert.IsInstanceOfType<ConflictObjectResult>(await controller.Purge("acme", "acme"));
		var list = ( await controller.List() as JsonResult )?.Value as List<TenantOverviewItem>;
		Assert.HasCount(1, list!);
	}
}
