using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Thumbnails;
using starsky.foundation.platform.Helpers;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices;

[TestClass]
public class SyncThumbnailTableAsyncTest
{
	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_WithRealDbContext()
	{
		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase(nameof(SyncThumbnailTableAsyncTest));
		var options = builderDb.Options;
		var dbContext = new ApplicationDbContext(options);

		const string fileHash = "SyncThumbnailTableAsyncTest_WithRealDbContext";

		dbContext.Thumbnails.Add(new ThumbnailItem(fileHash, true, true, null, null));
		await dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var sync = new SyncAddAddThumbnailTable(new ThumbnailQuery(dbContext, null,
			new FakeIWebLogger(), new FakeMemoryCache()));

		var content = await sync.SyncThumbnailTableAsync(
		[
			new FileIndexItem { FileHash = fileHash, IsDirectory = false, Status = FileIndexItem.ExifStatus.Ok }
		]);

		Assert.HasCount(1, content);

		// should not overwrite the existing data
		var item =
			await dbContext.Thumbnails.FirstOrDefaultAsync(p =>
				p.FileHash == fileHash, TestContext.CancellationTokenSource.Token);

		Assert.IsNotNull(item);
		Assert.IsTrue(item.TinyMeta);
		Assert.IsTrue(item.Small);
		Assert.IsNull(item.Large);
		Assert.IsNull(item.Large);
	}

	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_InvalidData_WithRealDbContext()
	{
		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase(nameof(SyncThumbnailTableAsyncTest) +
		                              "invalid_data");
		var options = builderDb.Options;
		var dbContext = new ApplicationDbContext(options);


		var sync =
			new SyncAddAddThumbnailTable(new ThumbnailQuery(dbContext, null, new FakeIWebLogger(),
				new FakeMemoryCache()));
		var content = await sync.SyncThumbnailTableAsync(
		[
			new FileIndexItem
			{
				FileHash = string.Empty, // <- invalid data (this line)
				IsDirectory = false,
				Status = FileIndexItem.ExifStatus.Ok
			},
			new FileIndexItem
			{
				FileHash = "hide",
				IsDirectory = true, // <- invalid data (this line)
				Status = FileIndexItem.ExifStatus.Ok
			},
			new FileIndexItem
			{
				FileHash = "hide",
				IsDirectory = false,
				Status = FileIndexItem.ExifStatus
					.NotFoundNotInIndex // <- invalid data (this line)
			},
			new FileIndexItem
			{
				FileHash = "duplicate",
				IsDirectory = false,
				Status = FileIndexItem.ExifStatus.Ok
			},
			new FileIndexItem
			{
				FileHash = "duplicate",
				IsDirectory = false,
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);

		Assert.HasCount(5, content);

		// should not overwrite the existing data
		var counter = await dbContext.Thumbnails.CountAsync(TestContext.CancellationTokenSource.Token);

		Assert.AreEqual(1, counter);

		var item = await dbContext.Thumbnails.FirstOrDefaultAsync(p => p.FileHash == "duplicate", TestContext.CancellationTokenSource.Token);

		Assert.IsNotNull(item);
		Assert.IsNull(item.TinyMeta);
		Assert.IsNull(item.Small);
	}

	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_WithNull()
	{
		var sync = new SyncAddAddThumbnailTable(new FakeIThumbnailQuery());
		Assert.IsEmpty(await sync.SyncThumbnailTableAsync(
			new List<FileIndexItem>()));
	}

	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_IgnoreXmpFile()
	{
		var query = new FakeIThumbnailQuery();
		var sync = new SyncAddAddThumbnailTable(query);

		Assert.HasCount(1, await sync.SyncThumbnailTableAsync(
		[
			new("/test.jpg")
			{
				Status = FileIndexItem.ExifStatus.Ok,
				ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
			}
		]));

		var result = await query.GetMissingThumbnailsBatchAsync(0, 100);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_KeepJpeg()
	{
		var query = new FakeIThumbnailQuery();
		var sync = new SyncAddAddThumbnailTable(query);

		Assert.HasCount(1, await sync.SyncThumbnailTableAsync(
		[
			new FileIndexItem("/test.jpg")
			{
				Status = FileIndexItem.ExifStatus.Ok,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		]));

		var result = await query.GetMissingThumbnailsBatchAsync(0, 100);
		Assert.IsEmpty(result);
	}

	public TestContext TestContext { get; set; }
}
