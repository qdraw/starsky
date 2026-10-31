using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

/// <summary>
///     Each answer of the tenant administration service has its own status code
/// </summary>
[TestClass]
public sealed class TenantsControllerResultTest
{
	private sealed class FakeTenantAdminService(TenantAdminResult result) : ITenantAdminService
	{
		public Task<List<TenantOverviewItem>> ListAsync()
		{
			return Task.FromResult<List<TenantOverviewItem>>([]);
		}

		public Task<TenantAdminResult> CreateAsync(string tenantId, string displayName,
			int creatorUserId, TenantRoots? roots = null)
		{
			return Task.FromResult(result);
		}

		public Task<TenantAdminResult> SetEnabledAsync(string tenantId, bool enabled)
		{
			return Task.FromResult(result);
		}

		public Task<TenantAdminResult> DeleteAsync(string tenantId)
		{
			return Task.FromResult(result);
		}

		public Task<TenantAdminResult> PurgeAsync(string tenantId)
		{
			return Task.FromResult(result);
		}
	}

	private static TenantsController Controller(TenantAdminResult result)
	{
		return new TenantsController(null!, new FakeTenantAdminService(result),
			new FakeIUserManger(new UserOverviewModel()), new TenantContext())
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
	}

	[TestMethod]
	[DataRow(TenantAdminResult.Ok, 200)]
	[DataRow(TenantAdminResult.NotFound, 404)]
	[DataRow(TenantAdminResult.InvalidId, 400)]
	[DataRow(TenantAdminResult.InvalidFolders, 400)]
	[DataRow(TenantAdminResult.DefaultTenant, 400)]
	[DataRow(TenantAdminResult.ProvisioningFailed, 500)]
	[DataRow(TenantAdminResult.AlreadyExists, 409)]
	[DataRow(TenantAdminResult.InvalidState, 409)]
	public async Task EveryAction_MapsTheResultToAStatusCode(TenantAdminResult result,
		int expected)
	{
		var controller = Controller(result);

		var results = new[]
		{
			await controller.Create(new CreateTenantRequest("acme", "Acme")),
			await controller.Disable("acme"),
			await controller.Enable("acme"),
			await controller.Delete("acme"),
			await controller.Purge("acme", "acme")
		};

		foreach ( var actionResult in results )
		{
			Assert.AreEqual(expected, ( actionResult as IStatusCodeActionResult )?.StatusCode);
		}
	}

	[TestMethod]
	public async Task Purge_WrongConfirmation_BadRequest()
	{
		var result = await Controller(TenantAdminResult.Ok).Purge("acme", "other");

		Assert.AreEqual(400, ( result as IStatusCodeActionResult )?.StatusCode);
	}

	[TestMethod]
	public async Task List_ReturnsJson()
	{
		var result = await Controller(TenantAdminResult.Ok).List();

		Assert.IsInstanceOfType<JsonResult>(result);
	}
}
