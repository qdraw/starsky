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
		await dbContext.SaveChangesAsync();

		var sync = new SyncAddAddThumbnailTable(new ThumbnailQuery(dbContext, null,
			new FakeIWebLogger(), new FakeMemoryCache()));

		var content = await sync.SyncThumbnailTableAsync(
			new List<FileIndexItem>
			{
				new()
				{
					FileHash = fileHash,
					IsDirectory = false,
					Status = FileIndexItem.ExifStatus.Ok
				}
			});

		Assert.AreEqual(1, content.Count);

		// should not overwrite the existing data
		var item =
			await dbContext.Thumbnails.FirstOrDefaultAsync(p =>
				p.FileHash == fileHash);

		Assert.IsNotNull(item);
		Assert.IsTrue(item.TinyMeta);
		Assert.IsTrue(item.Small);
		Assert.AreEqual(null, item.Large);
		Assert.AreEqual(null, item.Large);
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
			new List<FileIndexItem>
			{
				new()
				{
					FileHash = string.Empty, // <- invalid data (this line)
					IsDirectory = false,
					Status = FileIndexItem.ExifStatus.Ok
				},
				new()
				{
					FileHash = "hide",
					IsDirectory = true, // <- invalid data (this line)
					Status = FileIndexItem.ExifStatus.Ok
				},
				new()
				{
					FileHash = "hide",
					IsDirectory = false,
					Status = FileIndexItem.ExifStatus
						.NotFoundNotInIndex // <- invalid data (this line)
				},
				new()
				{
					FileHash = "duplicate",
					IsDirectory = false,
					Status = FileIndexItem.ExifStatus.Ok
				},
				new()
				{
					FileHash = "duplicate",
					IsDirectory = false,
					Status = FileIndexItem.ExifStatus.Ok
				}
			});

		Assert.AreEqual(5, content.Count);

		// should not overwrite the existing data
		var counter = await dbContext.Thumbnails.CountAsync();

		Assert.AreEqual(1, counter);

		var item = await dbContext.Thumbnails.FirstOrDefaultAsync(p => p.FileHash == "duplicate");

		Assert.IsNotNull(item);
		Assert.AreEqual(null, item.TinyMeta);
		Assert.AreEqual(null, item.Small);
	}

	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_WithNull()
	{
		var sync = new SyncAddAddThumbnailTable(new FakeIThumbnailQuery());
		Assert.AreEqual(0, ( await sync.SyncThumbnailTableAsync(
			new List<FileIndexItem>()) ).Count);
	}

	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_IgnoreXmpFile()
	{
		var query = new FakeIThumbnailQuery();
		var sync = new SyncAddAddThumbnailTable(query);

		Assert.AreEqual(1, ( await sync.SyncThumbnailTableAsync(
			new List<FileIndexItem>
			{
				new("/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.Ok,
					ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
				}
			}) ).Count);

		var result = await query.GetMissingThumbnailsBatchAsync(0, 100);
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public async Task SyncThumbnailTableAsyncTest_KeepJpeg()
	{
		var query = new FakeIThumbnailQuery();
		var sync = new SyncAddAddThumbnailTable(query);

		Assert.AreEqual(1, ( await sync.SyncThumbnailTableAsync(
			new List<FileIndexItem>
			{
				new("/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.Ok,
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
				}
			}) ).Count);

		var result = await query.GetMissingThumbnailsBatchAsync(0, 100);
		Assert.AreEqual(0, result.Count);
	}
}
