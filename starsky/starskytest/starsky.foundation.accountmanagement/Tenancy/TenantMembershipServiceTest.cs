using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

[TestClass]
public sealed class TenantMembershipServiceTest
{
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	///     owner@x (platform admin, Administrator of default), user@x (User of default)
	/// </summary>
	private static async Task<(int Owner, int Member)> SeedUsers(TenancyTestSetup setup)
	{
		await using var context = setup.ContextFor("default");
		var userManager = setup.UserManagerFor(context, "default");
		var owner = ( await userManager.SignUpAsync("Owner", "email", "owner@x.nl", "password-1234") )
			.User!;
		// a second sign-up is not the first user: default role (User)
		var member = ( await userManager.SignUpAsync("Member", "email", "user@x.nl", "password-1234") )
			.User!;
		return ( owner.Id, member.Id );
	}

	[TestMethod]
	public async Task FirstUser_IsPlatformAdminAndAdministrator_SecondIsNot()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, member) = await SeedUsers(setup);

		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);

		var ownerAccess = await service.GetAccessAsync(owner, "default");
		var memberAccess = await service.GetAccessAsync(member, "default");

		Assert.IsTrue(ownerAccess.IsPlatformAdmin);
		Assert.AreEqual("Administrator", ownerAccess.Membership?.RoleCode);
		Assert.Contains("AppSettingsWrite", ownerAccess.Membership!.Permissions);
		Assert.IsFalse(memberAccess.IsPlatformAdmin);
		Assert.AreEqual("User", memberAccess.Membership?.RoleCode);
		Assert.IsEmpty(memberAccess.Membership!.Permissions);
	}

	[TestMethod]
	public async Task SameUser_HasDifferentRoleInEachTenant()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);

		Assert.AreEqual(MembershipResult.Ok,
			await service.AddMemberAsync("beta", "user@x.nl", "Administrator"));

		Assert.AreEqual("User", ( await service.GetAccessAsync(member, "default") ).Membership?.RoleCode);
		Assert.AreEqual("Administrator",
			( await service.GetAccessAsync(member, "beta") ).Membership?.RoleCode);
		Assert.IsNull(( await service.GetAccessAsync(member, "gamma") ).Membership);
	}

	[TestMethod]
	public async Task Register_WithExistingEmail_DoesNotJoinTheTenant()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await setup.AddActiveTenant("beta");

		// someone registers the e-mail address of an existing user on the url of another tenant
		await using ( var betaContext = setup.ContextFor("beta") )
		{
			var betaManager = setup.UserManagerFor(betaContext, "beta");
			await betaManager.SignUpAsync("x", "email", "user@x.nl", "another-password-1");
		}

		await using var context = setup.ContextFor("default");
		Assert.IsNull(( await new TenantMembershipService(context).GetAccessAsync(member, "beta") )
			.Membership);
	}

	[TestMethod]
	public async Task AddMember_Results()
	{
		await using var setup = await TenancyTestSetup.Create();
		await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);

		Assert.AreEqual(MembershipResult.TenantNotFound,
			await service.AddMemberAsync("nope", "user@x.nl", "User"));
		Assert.AreEqual(MembershipResult.RoleNotFound,
			await service.AddMemberAsync("beta", "user@x.nl", "Root"));
		Assert.AreEqual(MembershipResult.UserNotFound,
			await service.AddMemberAsync("beta", "nobody@x.nl", "User"));
		Assert.AreEqual(MembershipResult.Ok,
			await service.AddMemberAsync("beta", "user@x.nl", "User"));
		Assert.AreEqual(MembershipResult.AlreadyMember,
			await service.AddMemberAsync("beta", "user@x.nl", "Administrator"));
	}

	[TestMethod]
	public async Task LastAdministrator_CanNotBeDemotedOrRemoved()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, member) = await SeedUsers(setup);
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);

		Assert.AreEqual(MembershipResult.LastAdministrator,
			await service.SetRoleAsync("default", owner, "User"));
		Assert.AreEqual(MembershipResult.LastAdministrator,
			await service.RemoveMemberAsync("default", owner));

		// with a second administrator it is allowed
		Assert.AreEqual(MembershipResult.Ok, await service.SetRoleAsync("default", member, "Administrator"));
		Assert.AreEqual(MembershipResult.Ok, await service.SetRoleAsync("default", owner, "User"));
		Assert.AreEqual(MembershipResult.LastAdministrator,
			await service.RemoveMemberAsync("default", member));
	}

	[TestMethod]
	public async Task SetRoleAndRemove_NotAMember()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);

		Assert.AreEqual(MembershipResult.NotAMember, await service.SetRoleAsync("beta", member, "User"));
		Assert.AreEqual(MembershipResult.NotAMember, await service.RemoveMemberAsync("beta", member));
		Assert.AreEqual(MembershipResult.RoleNotFound, await service.SetRoleAsync("default", member, "Root"));
	}

	[TestMethod]
	public async Task Revoke_IsVisibleAfterInvalidate_AndStaleWithoutIt()
	{
		await using var setup = await TenancyTestSetup.Create(true);
		var (_, member) = await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context, setup.Cache);
		await service.AddMemberAsync("beta", "user@x.nl", "User");
		Assert.IsNotNull(( await service.GetAccessAsync(member, "beta") ).Membership);

		// another instance removes the membership: this one still has the cached value (30 s)
		await using ( var other = setup.ContextFor("default") )
		{
			other.UserRoles.RemoveRange(other.UserRoles.Where(p => p.TenantId == "beta"));
			await other.SaveChangesAsync(TestContext.CancellationToken);
		}

		Assert.IsNotNull(( await service.GetAccessAsync(member, "beta") ).Membership);
		service.Invalidate(member, "beta");
		Assert.IsNull(( await service.GetAccessAsync(member, "beta") ).Membership);
	}

	[TestMethod]
	public async Task RemoveMember_ThroughService_RevokesImmediately()
	{
		await using var setup = await TenancyTestSetup.Create(true);
		var (_, member) = await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context, setup.Cache);
		await service.AddMemberAsync("beta", "user@x.nl", "User");
		Assert.IsNotNull(( await service.GetAccessAsync(member, "beta") ).Membership);

		await service.RemoveMemberAsync("beta", member);

		Assert.IsNull(( await service.GetAccessAsync(member, "beta") ).Membership);
	}

	[TestMethod]
	public async Task TenantsForUser_OnlyActiveTenants_WithRole()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await setup.AddActiveTenant("gamma", TenantState.Disabled);
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);
		await service.AddMemberAsync("beta", "user@x.nl", "Administrator");
		await service.AddMemberAsync("gamma", "user@x.nl", "User");

		var tenants = await service.GetTenantsForUserAsync(member);

		CollectionAssert.AreEqual(new[] { "beta", "default" },
			tenants.Select(t => t.TenantId).ToArray());
		Assert.AreEqual("Administrator", tenants.Single(t => t.TenantId == "beta").RoleCode);
		Assert.AreEqual("BETA", tenants.Single(t => t.TenantId == "beta").DisplayName);
	}

	[TestMethod]
	public async Task Members_OfOneTenantOnly()
	{
		await using var setup = await TenancyTestSetup.Create();
		await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		var service = new TenantMembershipService(context);
		await service.AddMemberAsync("beta", "user@x.nl", "User");

		var betaMembers = await service.GetMembersAsync("beta");
		var defaultMembers = await service.GetMembersAsync("default");

		Assert.HasCount(1, betaMembers);
		Assert.AreEqual("user@x.nl", betaMembers[0].Identifier);
		Assert.HasCount(2, defaultMembers);
	}

	[TestMethod]
	public async Task RemoveUser_RemovesAllMemberships()
	{
		await using var setup = await TenancyTestSetup.Create();
		await SeedUsers(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		await new TenantMembershipService(context).AddMemberAsync("beta", "user@x.nl", "User");

		var manager = setup.UserManagerFor(context, "default");
		var result = await manager.RemoveUser("email", "user@x.nl");

		Assert.IsTrue(result.Success);
		Assert.AreEqual(0, await context.UserRoles.CountAsync(p => p.User!.Name == "Member",
			TestContext.CancellationToken));
	}
}
