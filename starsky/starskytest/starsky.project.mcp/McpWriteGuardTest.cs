using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.mcp.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class McpWriteGuardTest
{
	private static readonly string[] SingleAPath = ["/a.jpg"];

	private static (McpWriteGuard guard, FakeIWebLogger logger) Create(
		AppSettings? appSettings = null, McpConfirmationStore? store = null,
		string user = "alice")
	{
		var logger = new FakeIWebLogger();
		var accessor = new FakeIHttpContextAccessor(user);
		var settings = appSettings ?? new AppSettings { McpAllowWrite = true };
		return (new McpWriteGuard(settings, store ?? new McpConfirmationStore(), accessor,
			logger), logger);
	}

	[TestMethod]
	public void Gate_WriteDisabled_Rejected()
	{
		var (guard, _) = Create(new AppSettings { McpAllowWrite = false });

		var result = guard.Gate("tool", ["/a.jpg"], "", null);

		Assert.AreEqual(McpGateDecision.Rejected, result.Decision);
	}

	[TestMethod]
	[DataRow("/../secret.jpg")]
	[DataRow("/a/../../b.jpg")]
	[DataRow("..")]
	[DataRow("/")]
	[DataRow("")]
	[DataRow("   ")]
	[DataRow("\\windows\\a.jpg")]
	[DataRow("/a.jpg;/b.jpg")]
	[DataRow("/a\n.jpg")]
	public void Gate_PathNotAllowed_Rejected(string path)
	{
		var (guard, _) = Create();

		var result = guard.Gate("tool", [path], "", null);

		Assert.AreEqual(McpGateDecision.Rejected, result.Decision);
	}

	[TestMethod]
	public void Gate_NoPaths_Rejected()
	{
		var (guard, _) = Create();

		Assert.AreEqual(McpGateDecision.Rejected, guard.Gate("tool", [], "", null).Decision);
		Assert.AreEqual(McpGateDecision.Rejected, guard.Gate("tool", null, "", null).Decision);
	}

	[TestMethod]
	public void Gate_ReadOnlyFolder_Rejected()
	{
		var (guard, _) = Create(new AppSettings
		{
			McpAllowWrite = true, ReadOnlyFolders = ["/readonly"]
		});

		var result = guard.Gate("tool", ["/readonly/a.jpg"], "", null);

		Assert.AreEqual(McpGateDecision.Rejected, result.Decision);
	}

	[TestMethod]
	public void Gate_OverMaxItems_Rejected()
	{
		var (guard, _) = Create(new AppSettings { McpAllowWrite = true, McpMaxItemsPerCall = 2 });

		var result = guard.Gate("tool", ["/a.jpg", "/b.jpg", "/c.jpg"], "", null);

		Assert.AreEqual(McpGateDecision.Rejected, result.Decision);
		Assert.Contains("maximum is 2", result.Message!);
	}

	[TestMethod]
	public void Gate_DuplicatePaths_CountOnce()
	{
		var (guard, _) = Create(new AppSettings { McpAllowWrite = true, McpMaxItemsPerCall = 1 });

		var result = guard.Gate("tool", ["/a.jpg", "a.jpg/"], "", null);

		Assert.AreEqual(McpGateDecision.NeedsConfirmation, result.Decision);
		Assert.AreSequenceEqual(SingleAPath, result.Paths);
	}

	[TestMethod]
	public void Gate_PreviewThenConfirm_Executes()
	{
		var store = new McpConfirmationStore();
		var (guard, logger) = Create(store: store);

		var preview = guard.Gate("tool", ["/a.jpg"], "c=false", null);
		Assert.AreEqual(McpGateDecision.NeedsConfirmation, preview.Decision);
		Assert.IsNotNull(preview.ConfirmToken);

		var confirmed = guard.Gate("tool", ["/a.jpg"], "c=false", preview.ConfirmToken);
		Assert.AreEqual(McpGateDecision.Confirmed, confirmed.Decision);

		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("phase=preview")));
		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("phase=execute")));
	}

	[TestMethod]
	public void Gate_TokenUsedTwice_RejectedSecondTime()
	{
		var (guard, _) = Create();
		var preview = guard.Gate("tool", ["/a.jpg"], "", null);

		guard.Gate("tool", ["/a.jpg"], "", preview.ConfirmToken);
		var second = guard.Gate("tool", ["/a.jpg"], "", preview.ConfirmToken);

		Assert.AreEqual(McpGateDecision.Rejected, second.Decision);
	}

	[TestMethod]
	public void Gate_TokenWithOtherPaths_Rejected()
	{
		var (guard, _) = Create();
		var preview = guard.Gate("tool", ["/a.jpg"], "", null);

		var result = guard.Gate("tool", ["/b.jpg"], "", preview.ConfirmToken);

		Assert.AreEqual(McpGateDecision.Rejected, result.Decision);
	}

	[TestMethod]
	public void Gate_TokenFromOtherUser_Rejected()
	{
		var store = new McpConfirmationStore();
		var (alice, _) = Create(store: store);
		var (bob, _) = Create(store: store, user: "bob");
		var preview = alice.Gate("tool", ["/a.jpg"], "", null);

		var result = bob.Gate("tool", ["/a.jpg"], "", preview.ConfirmToken);

		Assert.AreEqual(McpGateDecision.Rejected, result.Decision);
	}

	[TestMethod]
	public void Audit_WritesToLog()
	{
		var (guard, logger) = Create();

		guard.Audit("tool", ["/a.jpg"], "Ok");

		Assert.IsTrue(logger.TrackedInformation.Exists(p =>
			p.Item2!.Contains("user=alice") && p.Item2.Contains("phase=done")));
	}

	[TestMethod]
	public void Gate_NoHttpContext_UsesUnknownCaller()
	{
		var logger = new FakeIWebLogger();
		var guard = new McpWriteGuard(new AppSettings { McpAllowWrite = true },
			new McpConfirmationStore(), new FakeIHttpContextAccessor(null), logger);

		guard.Gate("tool", ["/a.jpg"], "", null);

		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("user=unknown")));
	}

	[TestMethod]
	public void Gate_NameIdentifierClaim_IsUsedAsCaller()
	{
		var logger = new FakeIWebLogger();
		var guard = new McpWriteGuard(new AppSettings { McpAllowWrite = true },
			new McpConfirmationStore(),
			new FakeIHttpContextAccessor("42", System.Security.Claims.ClaimTypes.NameIdentifier),
			logger);

		guard.Gate("tool", ["/a.jpg"], "", null);

		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("user=42")));
	}

	[TestMethod]
	public void Gate_NoHttpContext_CallerIsUnknown()
	{
		var logger = new FakeIWebLogger();
		var guard = new McpWriteGuard(new AppSettings { McpAllowWrite = true },
			new McpConfirmationStore(),
			new FakeIHttpContextAccessor { HttpContext = null }, logger);

		guard.Gate("tool", ["/a.jpg"], "", null);

		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("user=unknown")));
	}

	[TestMethod]
	public void Gate_NameIdentifierWinsOverName()
	{
		var logger = new FakeIWebLogger();
		var accessor = new FakeIHttpContextAccessor("alice");
		accessor.HttpContext!.User = new System.Security.Claims.ClaimsPrincipal(
			new System.Security.Claims.ClaimsIdentity([
				new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name,
					"alice"),
				new System.Security.Claims.Claim(
					System.Security.Claims.ClaimTypes.NameIdentifier, "7")
			], "test"));
		var guard = new McpWriteGuard(new AppSettings { McpAllowWrite = true },
			new McpConfirmationStore(), accessor, logger);

		guard.Gate("tool", ["/a.jpg"], "", null);

		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("user=7")));
	}

	[TestMethod]
	public void ValidateTarget_AllowedAndNotAllowed()
	{
		var (guard, _) = Create(new AppSettings
		{
			McpAllowWrite = true, ReadOnlyFolders = ["/readonly"]
		});

		Assert.AreEqual("/new/a.jpg", guard.ValidateTarget("new/a.jpg/"));
		Assert.IsNull(guard.ValidateTarget("/readonly/a.jpg"));
		Assert.IsNull(guard.ValidateTarget("/../a.jpg"));
		Assert.IsNull(guard.ValidateTarget(null));
	}

	[TestMethod]
	public void Gate_PrincipalWithoutIdentity_CallerIsUnknown()
	{
		var logger = new FakeIWebLogger();
		var accessor = new FakeIHttpContextAccessor(null);
		accessor.HttpContext!.User = new System.Security.Claims.ClaimsPrincipal();
		var guard = new McpWriteGuard(new AppSettings { McpAllowWrite = true },
			new McpConfirmationStore(), accessor, logger);

		guard.Gate("tool", ["/a.jpg"], "", null);

		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("user=unknown")));
	}
}
