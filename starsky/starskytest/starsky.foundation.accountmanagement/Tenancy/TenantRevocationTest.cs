using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.database.Models.Account;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

/// <summary>
///     Access that ends must reach what is already open (sockets) and may not be raced
/// </summary>
[TestClass]
public sealed class TenantRevocationTest
{
	public TestContext TestContext { get; set; } = null!;

	private static async Task<(int Owner, int Member)> SeedUsers(TenancyTestSetup setup)
	{
		await using var context = setup.ContextFor("default");
		var manager = setup.UserManagerFor(context, "default");
		var owner = ( await manager.SignUpAsync("o", "email", "owner@x.nl", "password-1234") ).User!;
		var member = ( await manager.SignUpAsync("m", "email", "user@x.nl", "password-1234") ).User!;
		return ( owner.Id, member.Id );
	}

	[TestMethod]
	public async Task RemoveMember_ClosesTheSocketsOfThatUserInThatTenant()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await using var context = setup.ContextFor("default");
		var sockets = new FakeIWebSocketConnectionsService();
		var service = new TenantMembershipService(context, null, sockets);

		Assert.AreEqual(MembershipResult.Ok, await service.RemoveMemberAsync("default", member));

		Assert.AreEqual(( "default", ( int? ) member, "Membership removed" ), sockets.FakeClosed.Single());
	}

	[TestMethod]
	public async Task RemoveMember_NotAllowed_ClosesNothing()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await SeedUsers(setup);
		await using var context = setup.ContextFor("default");
		var sockets = new FakeIWebSocketConnectionsService();
		var service = new TenantMembershipService(context, null, sockets);

		// the last administrator
		Assert.AreEqual(MembershipResult.LastAdministrator,
			await service.RemoveMemberAsync("default", owner));

		Assert.IsEmpty(sockets.FakeClosed);
	}

	[TestMethod]
	public async Task DisableAndDeleteTenant_CloseTheSocketsOfTheTenant()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await SeedUsers(setup);
		await using var context = setup.ContextFor("default");
		var sockets = new FakeIWebSocketConnectionsService();
		var service = new TenantAdminService(context, setup.Registry, setup.SettingsProvider,
			new FakeIWebLogger(), sockets);
		await service.CreateAsync("acme", "Acme", owner);
		await service.CreateAsync("beta", "Beta", owner);
		sockets.FakeClosed.Clear();

		await service.SetEnabledAsync("acme", false);
		await service.DeleteAsync("beta");
		// enabling again does not close anything
		await service.SetEnabledAsync("acme", true);

		CollectionAssert.AreEqual(new[] { "acme", "beta" },
			sockets.FakeClosed.Select(c => c.TenantId).ToArray());
		Assert.IsTrue(sockets.FakeClosed.All(c => c.UserId == null));
	}

	[TestMethod]
	public async Task RemoveUser_ClosesTheSocketsOfThatUserInEveryTenant()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await using var context = setup.ContextFor("default");
		var sockets = new FakeIWebSocketConnectionsService();
		var manager = new UserManager(context,
			setup.AppSettings, new FakeIWebLogger(), null, null, sockets);

		var result = await manager.RemoveUser("email", "user@x.nl");

		Assert.IsTrue(result.Success);
		Assert.AreEqual(( ( string? ) null, ( int? ) member, "User removed" ), sockets.FakeClosed.Single());
	}

	[TestMethod]
	public async Task OneRolePerUserPerTenant_IsEnforcedByTheDatabase()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await using var context = setup.ContextFor("default");
		var roles = await context.Roles.ToListAsync(TestContext.CancellationToken);
		var other = roles.First(r => r.Code == "Administrator");

		// a second row for the same user in the same tenant, with another role
		context.UserRoles.Add(new UserRole { UserId = member, RoleId = other.Id, TenantId = "default" });

		await Assert.ThrowsExactlyAsync<DbUpdateException>(() =>
			context.SaveChangesAsync(TestContext.CancellationToken));
	}

	[TestMethod]
	public async Task SetRole_StaysOneMembership_AndTakesEffect()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);

		Assert.AreEqual(MembershipResult.Ok,
			await service.SetRoleAsync("default", member, "Administrator"));
		Assert.AreEqual(MembershipResult.Ok, await service.SetRoleAsync("default", member, "User"));

		Assert.AreEqual(1, await context.UserRoles.CountAsync(p => p.UserId == member,
			TestContext.CancellationToken));
		Assert.AreEqual("User", ( await service.GetAccessAsync(member, "default") ).Membership?.RoleCode);
	}
}
