using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.project.mcp.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class McpWriteGuardTenantTest
{
	private static (McpWriteGuard Guard, FakeIWebLogger Logger) Create(McpConfirmationStore store,
		string tenantId, string user = "7")
	{
		var logger = new FakeIWebLogger();
		var tenant = new TenantContext();
		tenant.SetTenant(tenantId);
		TenantAmbient.Reset();
		return ( new McpWriteGuard(new AppSettings { McpAllowWrite = true }, store,
			new FakeIHttpContextAccessor(user), logger, tenant), logger );
	}

	[TestMethod]
	public void Preview_OfOneTenant_CanNotBeConfirmedInAnother_BySameAccount()
	{
		var store = new McpConfirmationStore();
		var (acme, _) = Create(store, "acme");
		var (beta, _) = Create(store, "beta");

		var preview = acme.Gate("rename", ["/a.jpg"], "x", null);
		var wrongTenant = beta.Gate("rename", ["/a.jpg"], "x", preview.ConfirmToken);

		Assert.AreEqual(McpGateDecision.NeedsConfirmation, preview.Decision);
		Assert.AreEqual(McpGateDecision.Rejected, wrongTenant.Decision);
	}

	[TestMethod]
	public void Preview_ConfirmedInTheSameTenant()
	{
		var store = new McpConfirmationStore();
		var (first, _) = Create(store, "acme");
		var (second, _) = Create(store, "acme");

		var preview = first.Gate("rename", ["/a.jpg"], "x", null);
		var confirmed = second.Gate("rename", ["/a.jpg"], "x", preview.ConfirmToken);

		Assert.AreEqual(McpGateDecision.Confirmed, confirmed.Decision);
	}

	[TestMethod]
	public void AuditLog_NamesTheTenant_ExceptTheDefaultOne()
	{
		var store = new McpConfirmationStore();
		var (acme, acmeLog) = Create(store, "acme");
		var (def, defLog) = Create(store, "default");

		acme.Gate("rename", ["/a.jpg"], "x", null);
		def.Gate("rename", ["/a.jpg"], "x", null);

		Assert.IsTrue(acmeLog.TrackedInformation.Exists(p => p.Item2?.Contains("user=acme/7") == true));
		Assert.IsTrue(defLog.TrackedInformation.Exists(p => p.Item2?.Contains("user=7 ") == true));
	}
}
