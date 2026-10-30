using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.metaupdate.Services;

[TestClass]
public sealed class MetaUpdateRunnerTest
{
	private static (MetaUpdateRunner runner, FakeIUpdateBackgroundTaskQueue queue,
		FakeIMetaUpdateConnectionService connection) Create()
	{
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var connection = new FakeIMetaUpdateConnectionService();
		var runner = new MetaUpdateRunner(new FakeMetaPreflight(), new FakeIMetaUpdateService(),
			queue, new FakeIWebLogger(), connection);
		return ( runner, queue, connection );
	}

	[TestMethod]
	public async Task UpdateAsync_Found_QueuesJobAndSendsToSocket()
	{
		var (runner, queue, connection) = Create();

		var result = await runner.UpdateAsync(
			new FileIndexItem("/a.jpg") { Tags = "x" }, ["/a.jpg"], false, true, 0);

		Assert.IsFalse(result.AllNotFound);
		Assert.HasCount(1, result.Updated!);
		Assert.AreNotSame(result.Results[0], result.Updated[0]);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
		Assert.HasCount(1, connection.Sent);
	}

	[TestMethod]
	public async Task UpdateAsync_WithCurrentActivity_PassesTraceParent()
	{
		var (runner, queue, _) = Create();
		using var activity = new Activity("mcp-test");
		activity.Start();

		await runner.UpdateAsync(new FileIndexItem("/a.jpg") { Tags = "x" }, ["/a.jpg"],
			false, true, 0);

		Assert.AreEqual(activity.Id, queue.LastQueuedJob!.TraceParentId);
	}

	[TestMethod]
	public async Task UpdateAsync_NothingFound_NoSocketAndNoUpdatedItems()
	{
		var (runner, queue, connection) = Create();

		var result = await runner.UpdateAsync(
			new FileIndexItem("/a.jpg")
			{
				Tags = "x", Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
			}, ["/a.jpg"], false, true, 0);

		Assert.IsTrue(result.AllNotFound);
		Assert.IsNull(result.Updated);
		Assert.HasCount(1, result.Results);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
		Assert.IsEmpty(connection.Sent);
	}
}
