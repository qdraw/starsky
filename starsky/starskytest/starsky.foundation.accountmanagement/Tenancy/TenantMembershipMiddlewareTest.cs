using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.platform.Tenancy;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

[TestClass]
public sealed class TenantMembershipMiddlewareTest
{
	public TestContext TestContext { get; set; } = null!;

	private sealed record Result(HttpContext Context, bool NextCalled);

	private async Task<Result> Run(TenancyTestSetup setup, string tenantId, string path,
		int? userId, params Claim[] extraClaims)
	{
		await using var dbContext = setup.ContextFor("default");
		var services = new ServiceCollection();
		var tenantContext = new TenantContext();
		tenantContext.SetTenant(tenantId);
		services.AddSingleton<ITenantContext>(tenantContext);
		services.AddSingleton<ITenantMembershipService>(new TenantMembershipService(dbContext));
		await using var provider = services.BuildServiceProvider();

		var context = new DefaultHttpContext { RequestServices = provider };
		context.Request.Path = path;
		context.Response.Body = new System.IO.MemoryStream();
		if ( userId != null )
		{
			var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }
				.Concat(extraClaims);
			context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie"));
		}

		var nextCalled = false;
		await new TenantMembershipMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		}).Invoke(context);
		return new Result(context, nextCalled);
	}

	private async Task<(int Owner, int Member)> Seed(TenancyTestSetup setup)
	{
		await using var context = setup.ContextFor("default");
		var manager = setup.UserManagerFor(context, "default");
		var owner = ( await manager.SignUpAsync("o", "email", "owner@x.nl", "password-1234") ).User!;
		var member = ( await manager.SignUpAsync("m", "email", "user@x.nl", "password-1234") ).User!;
		return ( owner.Id, member.Id );
	}

	[TestMethod]
	public async Task NonMember_Api_Forbidden()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await Seed(setup);
		await setup.AddActiveTenant("beta");

		var result = await Run(setup, "beta", "/api/index", member);

		Assert.IsFalse(result.NextCalled);
		Assert.AreEqual(403, result.Context.Response.StatusCode);
	}

	[TestMethod]
	public async Task NonMember_Realtime_Forbidden()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await Seed(setup);
		await setup.AddActiveTenant("beta");

		var result = await Run(setup, "beta", "/realtime", member);

		Assert.AreEqual(403, result.Context.Response.StatusCode);
	}

	[TestMethod]
	[DataRow("/api/account/status")]
	[DataRow("/api/account/login")]
	[DataRow("/api/tenants/mine")]
	[DataRow("/api/health")]
	[DataRow("/")]
	[DataRow("/assets/app.js")]
	public async Task NonMember_ExemptPaths_Pass_WithoutRoleClaims(string path)
	{
		await using var setup = await TenancyTestSetup.Create();
		var (_, member) = await Seed(setup);
		await setup.AddActiveTenant("beta");

		var result = await Run(setup, "beta", path, member,
			new Claim(ClaimTypes.Role, "Administrator"));

		Assert.IsTrue(result.NextCalled);
		Assert.IsFalse(result.Context.User.HasClaim(c => c.Type == ClaimTypes.Role),
			"the stale role from the cookie must be gone");
	}

	[TestMethod]
	public async Task Member_GetsRoleAndPermissionsOfThisTenant_StaleClaimsReplaced()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await Seed(setup);
		await setup.AddActiveTenant("beta");
		await using ( var context = setup.ContextFor("default") )
		{
			// Administrator in default, only User in beta
			await new TenantMembershipService(context).AddMemberAsync("beta", "owner@x.nl", "User");
		}

		var result = await Run(setup, "beta", "/api/index", owner,
			new Claim(ClaimTypes.Role, "Administrator"),
			new Claim(TenantMembershipMiddleware.PermissionClaim, "AppSettingsWrite"));

		Assert.IsTrue(result.NextCalled);
		var user = result.Context.User;
		Assert.IsTrue(user.HasClaim(ClaimTypes.Role, "User"));
		Assert.IsFalse(user.HasClaim(ClaimTypes.Role, "Administrator"));
		Assert.IsFalse(user.HasClaim(c => c.Type == TenantMembershipMiddleware.PermissionClaim));
		Assert.IsTrue(user.HasClaim(TenantMembershipMiddleware.PlatformAdminClaim, "true"));
		Assert.AreEqual(owner.ToString(), user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
	}

	[TestMethod]
	public async Task Member_InDefault_AsAdministrator_HasPermission()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await Seed(setup);

		var result = await Run(setup, "default", "/api/index", owner);

		Assert.IsTrue(result.Context.User.HasClaim(ClaimTypes.Role, "Administrator"));
		Assert.IsTrue(result.Context.User.HasClaim(TenantMembershipMiddleware.PermissionClaim,
			"AppSettingsWrite"));
	}

	[TestMethod]
	public async Task PlatformAdmin_WithoutMembership_NoPhotoAccess()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await Seed(setup);
		await setup.AddActiveTenant("beta");

		var result = await Run(setup, "beta", "/api/index", owner);

		Assert.AreEqual(403, result.Context.Response.StatusCode);

		var tenants = await Run(setup, "beta", "/api/tenants", owner);
		Assert.IsTrue(tenants.NextCalled);
		Assert.IsTrue(tenants.Context.User.HasClaim(TenantMembershipMiddleware.PlatformAdminClaim,
			"true"));
	}

	[TestMethod]
	public async Task Anonymous_Passes_Untouched()
	{
		await using var setup = await TenancyTestSetup.Create();

		var result = await Run(setup, "beta", "/api/index", null);

		Assert.IsTrue(result.NextCalled);
		Assert.IsFalse(result.Context.User.Identity?.IsAuthenticated);
	}

	[TestMethod]
	public void RequiresMembership_Paths()
	{
		Assert.IsTrue(TenantMembershipMiddleware.RequiresMembership("/api/index"));
		Assert.IsTrue(TenantMembershipMiddleware.RequiresMembership("/realtime"));
		Assert.IsTrue(TenantMembershipMiddleware.RequiresMembership("/api/Tenantsx"));
		Assert.IsFalse(TenantMembershipMiddleware.RequiresMembership("/api/tenants/mine"));
		Assert.IsFalse(TenantMembershipMiddleware.RequiresMembership("/api/account/status"));
		Assert.IsFalse(TenantMembershipMiddleware.RequiresMembership(""));
	}
}
