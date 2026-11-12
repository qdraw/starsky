using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

/// <summary>
///     The unhappy paths of the tenant services: data that was changed by hand, a repeated
///     transaction, a folder that can not be made
/// </summary>
[TestClass]
public sealed class TenantEdgeCasesTest
{
	public TestContext TestContext { get; set; } = null!;

	private const int MissingId = 9999;

	private static async Task<(int Owner, int Member)> Seed(TenancyTestSetup setup)
	{
		await using var context = setup.ContextFor("default");
		var manager = setup.UserManagerFor(context, "default");
		var owner = ( await manager.SignUpAsync("o", "email", "owner@x.nl", "password-1234") ).User!;
		var member = ( await manager.SignUpAsync("m", "email", "user@x.nl", "password-1234") ).User!;
		return ( owner.Id, member.Id );
	}

	private static TenantAdminService AdminService(TenancyTestSetup setup,
		ApplicationDbContext context)
	{
		return new TenantAdminService(context, setup.Registry, setup.SettingsProvider, new FakeIWebLogger(), null,
			setup.AppSettings);
	}

	[TestMethod]
	public async Task GetAccess_UnknownUser_NoMembership()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");

		var access = await new TenantMembershipService(context)
			.GetAccessAsync(MissingId, "default");

		Assert.IsNull(access.Membership);
		Assert.IsFalse(access.IsPlatformAdmin);
	}

	[TestMethod]
	public async Task SetLastUsed_MembershipOfAMissingUser_UserNotFound()
	{
		await using var setup = await TenancyTestSetup.Create();
		await Seed(setup);
		await setup.DisableForeignKeys();
		await using var context = setup.ContextFor("default");
		context.UserRoles.Add(new UserRole
		{
			UserId = MissingId, RoleId = 1, TenantId = "default"
		});
		await context.SaveChangesAsync(TestContext.CancellationToken);

		var result = await new TenantMembershipService(context)
			.SetLastUsedAsync(MissingId, "default");

		Assert.AreEqual(MembershipResult.UserNotFound, result);
	}

	[TestMethod]
	public async Task SetRole_NotAMember_NotAMember()
	{
		await using var setup = await TenancyTestSetup.Create();
		await Seed(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");

		var result = await new TenantMembershipService(context)
			.SetRoleAsync("beta", MissingId, "User");

		Assert.AreEqual(MembershipResult.NotAMember, result);
	}

	[TestMethod]
	public async Task AddMember_InsideATransactionOfTheCaller_UsesIt()
	{
		await using var setup = await TenancyTestSetup.Create();
		await Seed(setup);
		await setup.AddActiveTenant("beta");
		await using var context = setup.ContextFor("default");
		await using var transaction =
			await context.Database.BeginTransactionAsync(TestContext.CancellationToken);

		var result = await new TenantMembershipService(context)
			.AddMemberAsync("beta", "user@x.nl", "User");
		await transaction.CommitAsync(TestContext.CancellationToken);

		Assert.AreEqual(MembershipResult.Ok, result);
	}

	private sealed class TransientTestException : Exception;

	private sealed class FailOnce : SaveChangesInterceptor
	{
		public int Failures;

		public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
			DbContextEventData eventData, InterceptionResult<int> result,
			CancellationToken cancellationToken = default)
		{
			if ( Failures++ == 0 )
			{
				throw new TransientTestException();
			}

			return base.SavingChangesAsync(eventData, result, cancellationToken);
		}
	}

	private sealed class RetryTestExceptions(ExecutionStrategyDependencies dependencies)
		: ExecutionStrategy(dependencies, 2, TimeSpan.FromMilliseconds(10))
	{
		protected override bool ShouldRetryOn(Exception exception)
		{
			return exception is TransientTestException;
		}
	}

	[TestMethod]
	public async Task AddMember_TransientFailure_IsRepeatedByTheExecutionStrategy()
	{
		await using var setup = await TenancyTestSetup.Create();
		await Seed(setup);
		await setup.AddActiveTenant("beta");
		var interceptor = new FailOnce();
		await using var context = setup.ContextFor("default", (builder, connection) => builder
			.UseSqlite(connection, o => o.ExecutionStrategy(d => new RetryTestExceptions(d)))
			.AddInterceptors(interceptor));

		var result = await new TenantMembershipService(context)
			.AddMemberAsync("beta", "user@x.nl", "User");

		Assert.AreEqual(MembershipResult.Ok, result);
		Assert.AreEqual(2, interceptor.Failures, "failed once, then the repeat saved");
	}

	[TestMethod]
	public async Task Create_BlankDisplayName_UsesTheId()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await Seed(setup);
		await using var context = setup.ContextFor("default");

		Assert.AreEqual(TenantAdminResult.Ok,
			await AdminService(setup, context).CreateAsync("acme", "   ", owner));

		Assert.AreEqual("acme",
			( await context.Tenants.SingleAsync(TestContext.CancellationToken) ).DisplayName);
	}

	[TestMethod]
	public async Task Create_FoldersCanNotBeMade_ProvisioningFailed_TenantStaysInactive()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await Seed(setup);
		await using var context = setup.ContextFor("default");

		// a folder below a file can not be created
		var parent = Path.Combine(Path.GetTempPath(), "starsky-file-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(parent);
		var file = Path.Combine(parent, "file.txt");
		await File.WriteAllTextAsync(file, "x", TestContext.CancellationToken);
		try
		{
			var result = await AdminService(setup, context).CreateAsync("acme", "x", owner,
				new TenantRoots(Path.Combine(file, "storage"), Path.Combine(file, "thumbs"),
					Path.Combine(file, "temp")));

			Assert.AreEqual(TenantAdminResult.ProvisioningFailed, result);
			Assert.IsFalse(setup.Registry.IsActive("acme"));
		}
		finally
		{
			Directory.Delete(parent, true);
		}
	}

	[TestMethod]
	public async Task Create_CreatorDoesNotExist_TenantIsActiveWithoutAdministrator()
	{
		await using var setup = await TenancyTestSetup.Create();
		await Seed(setup);
		await using var context = setup.ContextFor("default");

		var result = await AdminService(setup, context).CreateAsync("acme", "x", MissingId);

		Assert.AreEqual(TenantAdminResult.Ok, result);
		Assert.IsFalse(await context.UserRoles.AnyAsync(p => p.TenantId == "acme",
			TestContext.CancellationToken));
	}

	[TestMethod]
	public async Task Delete_AlreadyPurged_InvalidState()
	{
		await using var setup = await TenancyTestSetup.Create();
		var (owner, _) = await Seed(setup);
		await using var context = setup.ContextFor("default");
		var service = AdminService(setup, context);
		await service.CreateAsync("acme", "x", owner);
		await service.DeleteAsync("acme");
		Assert.AreEqual(TenantAdminResult.Ok, await service.PurgeAsync("acme"));

		Assert.AreEqual(TenantAdminResult.InvalidState, await service.DeleteAsync("acme"));
	}

	private static AuthorizationFilterContext ContextWithoutIdentity()
	{
		var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };
		return new AuthorizationFilterContext(
			new ActionContext(httpContext, new RouteData(), new ActionDescriptor()), []);
	}

	[TestMethod]
	public void Attributes_PrincipalWithoutIdentity_Unauthorized()
	{
		var admin = ContextWithoutIdentity();
		new TenantAdminAttribute().OnAuthorization(admin);
		var platform = ContextWithoutIdentity();
		new PlatformAdminAttribute().OnAuthorization(platform);

		Assert.AreEqual(401, ( admin.Result as ContentResult )?.StatusCode);
		Assert.AreEqual(401, ( platform.Result as ContentResult )?.StatusCode);
	}

	private static async Task<bool> RunMiddleware(TenancyTestSetup setup, ClaimsPrincipal user)
	{
		await using var dbContext = setup.ContextFor("default");
		var services = new ServiceCollection();
		var tenantContext = new TenantContext();
		tenantContext.SetTenant("default");
		services.AddSingleton<ITenantContext>(tenantContext);
		services.AddSingleton<ITenantMembershipService>(new TenantMembershipService(dbContext));
		await using var provider = services.BuildServiceProvider();

		var context = new DefaultHttpContext { RequestServices = provider, User = user };
		context.Request.Path = "/api/index";
		var nextCalled = false;
		await new TenantMembershipMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		}).Invoke(context);
		return nextCalled;
	}

	[TestMethod]
	public async Task Middleware_PrincipalWithoutIdentity_PassesOn()
	{
		await using var setup = await TenancyTestSetup.Create();
		Assert.IsTrue(await RunMiddleware(setup, new ClaimsPrincipal()));
	}

	[TestMethod]
	[DataRow("0")]
	[DataRow("-3")]
	[DataRow("not-a-number")]
	public async Task Middleware_NoUsableUserId_PassesOn(string id)
	{
		await using var setup = await TenancyTestSetup.Create();
		var user = new ClaimsPrincipal(new ClaimsIdentity(
			[new Claim(ClaimTypes.NameIdentifier, id)], "cookie"));

		Assert.IsTrue(await RunMiddleware(setup, user));
	}
}
