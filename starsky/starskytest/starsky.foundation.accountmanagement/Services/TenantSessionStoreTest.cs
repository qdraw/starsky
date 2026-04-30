using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.accountmanagement.Services;

[TestClass]
public class TenantSessionStoreTest : DatabaseTest
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task ActivateTenantAsync_MultipleTenants_SameSession()
	{
		var user = new User { Name = "tenant-user", Created = DateTime.UtcNow };
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);

		var tenantA = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		var tenantB = new Tenant
		{
			Slug = "second",
			Name = "second",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddRangeAsync([tenantA, tenantB],
			TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var sut = new TenantSessionStore(DbContext);
		var session = await sut.CreateOrRefreshSessionAsync(user.Id);

		await sut.ActivateTenantAsync(session.Id, tenantA.Id);
		await sut.ActivateTenantAsync(session.Id, tenantB.Id);

		Assert.IsTrue(await sut.IsTenantActivatedAsync(session.Id, tenantA.Id));
		Assert.IsTrue(await sut.IsTenantActivatedAsync(session.Id, tenantB.Id));
		Assert.AreEqual(2,
			await DbContext.WebSessionTenants.CountAsync(w => w.WebSessionId == session.Id,
				TestContext.CancellationTokenSource.Token));
	}

	[TestMethod]
	public async Task RevokeSessionAsync_ThenSessionIsInvalid()
	{
		var user = new User { Name = "revoke-user", Created = DateTime.UtcNow };
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var sut = new TenantSessionStore(DbContext);
		var session = await sut.CreateOrRefreshSessionAsync(user.Id);

		await sut.RevokeSessionAsync(session.Id);

		var sessionAfterRevoke = await sut.GetValidSessionAsync(session.SessionId);
		Assert.IsNull(sessionAfterRevoke);
	}
}
