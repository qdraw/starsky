using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.mcp.Models;
using starsky.project.mcp.Services;
using starsky.project.mcp.Tools;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class StarskyWriteToolsTest
{
	private static (StarskyWriteTools tools, FakeIWebLogger logger) Create(
		AppSettings? appSettings = null)
	{
		var items = new List<FileIndexItem>
		{
			new("/a.jpg") { Status = FileIndexItem.ExifStatus.Ok },
			new("/b.jpg") { Status = FileIndexItem.ExifStatus.Ok }
		};
		var logger = new FakeIWebLogger();
		var accessor = new FakeIHttpContextAccessor();
		var guard = new McpWriteGuard(appSettings ?? new AppSettings { McpAllowWrite = true },
			new McpConfirmationStore(), accessor, logger);
		return (new StarskyWriteTools(guard, new FakeIMetaInfo(items),
			new FakeIMoveToTrashService(items)), logger);
	}

	[TestMethod]
	public async Task MoveToTrash_FirstCall_IsPreviewAndChangesNothing()
	{
		var (tools, logger) = Create();

		var result = await tools.MoveToTrash(["/a.jpg"]);

		Assert.AreEqual(McpWriteOutcome.Preview, result.Status);
		Assert.IsNotNull(result.ConfirmToken);
		Assert.AreEqual(1, result.Items.Count);
		Assert.IsFalse(logger.TrackedInformation.Exists(p => p.Item2!.Contains("phase=done")));
	}

	[TestMethod]
	public async Task MoveToTrash_WithToken_Executes()
	{
		var (tools, logger) = Create();
		var preview = await tools.MoveToTrash(["/a.jpg", "/b.jpg"], true);

		var result = await tools.MoveToTrash(["/a.jpg", "/b.jpg"], true, preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Executed, result.Status);
		Assert.AreEqual(2, result.Items.Count);
		Assert.Contains("2 existing", result.Message);
		Assert.IsNull(result.ConfirmToken);
		Assert.IsTrue(logger.TrackedInformation.Exists(p => p.Item2!.Contains("phase=done")));
	}

	[TestMethod]
	public async Task MoveToTrash_TokenWithChangedCollections_Rejected()
	{
		var (tools, _) = Create();
		var preview = await tools.MoveToTrash(["/a.jpg"], false);

		var result = await tools.MoveToTrash(["/a.jpg"], true, preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
	}

	[TestMethod]
	public async Task MoveToTrash_WriteDisabled_Rejected()
	{
		var (tools, _) = Create(new AppSettings { McpAllowWrite = false });

		var result = await tools.MoveToTrash(["/a.jpg"]);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
		Assert.IsNull(result.ConfirmToken);
	}

	[TestMethod]
	public async Task MoveToTrash_PathTraversal_Rejected()
	{
		var (tools, _) = Create();

		var result = await tools.MoveToTrash(["/../etc/passwd"]);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
	}
}
