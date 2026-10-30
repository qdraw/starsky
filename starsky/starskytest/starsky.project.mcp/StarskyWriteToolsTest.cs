using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.mcp.Models;
using starsky.project.mcp.Services;
using starsky.project.mcp.Tools;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class StarskyWriteToolsTest
{
	private sealed class Setup
	{
		public required StarskyWriteTools Tools { get; init; }
		public required FakeIWebLogger Logger { get; init; }
		public required FakeIMetaUpdateRunner Runner { get; init; }
		public required FakeIStorage Storage { get; init; }
		public required FakeINotificationQuery Notifications { get; init; }
	}

	private static Setup CreateSetup(AppSettings? appSettings = null)
	{
		var items = new List<FileIndexItem>
		{
			new("/a.jpg") { Status = FileIndexItem.ExifStatus.Ok },
			new("/b.jpg") { Status = FileIndexItem.ExifStatus.Ok }
		};
		var logger = new FakeIWebLogger();
		var settings = appSettings ?? new AppSettings { McpAllowWrite = true };
		var guard = new McpWriteGuard(settings, new McpConfirmationStore(),
			new FakeIHttpContextAccessor(), logger);
		var runner = new FakeIMetaUpdateRunner(items);
		var storage = new FakeIStorage(["/"], ["/a.jpg", "/b.jpg"]);
		var notifications = new FakeINotificationQuery();
		var tools = new StarskyWriteTools(guard, new FakeIMetaInfo(items),
			new FakeIMoveToTrashService(items), runner,
			new FakeIQuery(items), new FakeSelectorStorage(storage), notifications,
			new FakeIWebSocketConnectionsService(), logger);
		return new Setup
		{
			Tools = tools,
			Logger = logger,
			Runner = runner,
			Storage = storage,
			Notifications = notifications
		};
	}

	private static (StarskyWriteTools tools, FakeIWebLogger logger) Create(
		AppSettings? appSettings = null)
	{
		var setup = CreateSetup(appSettings);
		return ( setup.Tools, setup.Logger );
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

	[TestMethod]
	public async Task UpdateMetadata_NothingToChange_Rejected()
	{
		var setup = CreateSetup();

		var result = await setup.Tools.UpdateMetadata(["/a.jpg"], " ");

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
		Assert.IsNull(result.ConfirmToken);
	}

	[TestMethod]
	[DataRow(-1)]
	[DataRow(9)]
	public async Task UpdateMetadata_ColorClassOutOfRange_Rejected(int colorClass)
	{
		var setup = CreateSetup();

		var result = await setup.Tools.UpdateMetadata(["/a.jpg"], colorClass: colorClass);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
	}

	[TestMethod]
	public async Task UpdateMetadata_Preview_ChangesNothing()
	{
		var setup = CreateSetup();

		var result = await setup.Tools.UpdateMetadata(["/a.jpg"], "holiday", colorClass: 2);

		Assert.AreEqual(McpWriteOutcome.Preview, result.Status);
		Assert.IsNotNull(result.ConfirmToken);
		Assert.Contains("holiday", result.Message);
		Assert.Contains("colorClass 2", result.Message);
		Assert.AreEqual(0, setup.Runner.Calls.Count);
	}

	[TestMethod]
	public async Task UpdateMetadata_WithToken_Executes()
	{
		var setup = CreateSetup();
		var preview = await setup.Tools.UpdateMetadata(["/a.jpg", "/b.jpg"], "holiday",
			title: "Title", description: "Desc", colorClass: 3, append: true);

		var result = await setup.Tools.UpdateMetadata(["/a.jpg", "/b.jpg"], "holiday",
			title: "Title", description: "Desc", colorClass: 3, append: true,
			confirmToken: preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Executed, result.Status);
		Assert.AreEqual(2, result.Items.Count);
		var call = setup.Runner.Calls.Single();
		Assert.AreEqual("holiday", call.Model.Tags);
		Assert.AreEqual("Title", call.Model.Title);
		Assert.AreEqual("Desc", call.Model.Description);
		Assert.AreEqual(ColorClassParser.Color.Superior, call.Model.ColorClass);
		Assert.IsTrue(call.Append);
		CollectionAssert.AreEqual(new[] { "/a.jpg", "/b.jpg" }, call.Paths);
	}

	[TestMethod]
	public async Task UpdateMetadata_ColorClassNotGiven_DoesNotChangeColor()
	{
		var setup = CreateSetup();
		var preview = await setup.Tools.UpdateMetadata(["/a.jpg"], "x");

		await setup.Tools.UpdateMetadata(["/a.jpg"], "x", confirmToken: preview.ConfirmToken);

		Assert.AreEqual(ColorClassParser.Color.DoNotChange,
			setup.Runner.Calls.Single().Model.ColorClass);
	}

	[TestMethod]
	public async Task UpdateMetadata_TokenWithOtherTags_RejectedAndNothingRuns()
	{
		var setup = CreateSetup();
		var preview = await setup.Tools.UpdateMetadata(["/a.jpg"], "holiday");

		var result = await setup.Tools.UpdateMetadata(["/a.jpg"], "other",
			confirmToken: preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
		Assert.AreEqual(0, setup.Runner.Calls.Count);
	}

	[TestMethod]
	public async Task UpdateMetadata_FileNotFound_RejectedAfterConfirm()
	{
		var setup = CreateSetup();
		var preview = await setup.Tools.UpdateMetadata(["/missing.jpg"], "x");

		var result = await setup.Tools.UpdateMetadata(["/missing.jpg"], "x",
			confirmToken: preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
		Assert.Contains("None of the files", result.Message);
	}

	[TestMethod]
	public async Task UpdateMetadata_WriteDisabled_Rejected()
	{
		var setup = CreateSetup(new AppSettings { McpAllowWrite = false });

		var result = await setup.Tools.UpdateMetadata(["/a.jpg"], "x");

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
	}

	[TestMethod]
	[DataRow("/../x.jpg")]
	[DataRow("/")]
	[DataRow("")]
	public async Task Rename_TargetNotAllowed_Rejected(string to)
	{
		var setup = CreateSetup();

		var result = await setup.Tools.Rename("/a.jpg", to);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
		Assert.IsNull(result.ConfirmToken);
	}

	[TestMethod]
	public async Task Rename_Preview_ChangesNothing()
	{
		var setup = CreateSetup();

		var result = await setup.Tools.Rename("/a.jpg", "/c.jpg");

		Assert.AreEqual(McpWriteOutcome.Preview, result.Status);
		Assert.Contains("/a.jpg will be renamed to /c.jpg", result.Message);
		Assert.IsTrue(setup.Storage.ExistFile("/a.jpg"));
		Assert.IsFalse(setup.Storage.ExistFile("/c.jpg"));
		Assert.AreEqual(0, setup.Notifications.FakeContent.Count);
	}

	[TestMethod]
	public async Task Rename_WithToken_RenamesAndNotifies()
	{
		var setup = CreateSetup();
		var preview = await setup.Tools.Rename("/a.jpg", "/c.jpg");

		var result = await setup.Tools.Rename("/a.jpg", "/c.jpg",
			confirmToken: preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Executed, result.Status);
		Assert.IsFalse(setup.Storage.ExistFile("/a.jpg"));
		Assert.IsTrue(setup.Storage.ExistFile("/c.jpg"));
		Assert.AreEqual(true, setup.Notifications.Added);
	}

	[TestMethod]
	public async Task Rename_TokenWithOtherTarget_RejectedAndNothingMoves()
	{
		var setup = CreateSetup();
		var preview = await setup.Tools.Rename("/a.jpg", "/c.jpg");

		var result = await setup.Tools.Rename("/a.jpg", "/d.jpg",
			confirmToken: preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
		Assert.IsTrue(setup.Storage.ExistFile("/a.jpg"));
		Assert.IsFalse(setup.Storage.ExistFile("/d.jpg"));
	}

	[TestMethod]
	public async Task Rename_TargetExists_RejectedWithoutNotification()
	{
		var setup = CreateSetup();
		var preview = await setup.Tools.Rename("/a.jpg", "/b.jpg");

		var result = await setup.Tools.Rename("/a.jpg", "/b.jpg",
			confirmToken: preview.ConfirmToken);

		Assert.AreEqual(McpWriteOutcome.Rejected, result.Status);
		Assert.IsTrue(setup.Storage.ExistFile("/a.jpg"));
		Assert.AreNotEqual(true, setup.Notifications.Added);
	}
}
