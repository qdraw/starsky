using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.metaupdate.Services;

[TestClass]
public sealed class ReplaceServiceTest
{
	private readonly MetaReplaceService _metaReplace;
	private readonly Query _query;

	public ReplaceServiceTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();

		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(MetaReplaceService));
		var options = builder.Options;
		var dbContext = new ApplicationDbContext(options);
		_query = new Query(dbContext, new AppSettings(), null!,
			new FakeIWebLogger(), memoryCache);

		var iStorage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				"/test.jpg",
				"/test2.jpg",
				"/readonly/test.jpg",
				"/test.dng",
				"/test34598.jpg",
				"/test5.dng",
				"/test5.xmp",
				"/test_ok_and_same.jpg",
				"/test_deleted_and_same.jpg"
			});
		_metaReplace = new MetaReplaceService(_query,
			new AppSettings { ReadOnlyFolders = new List<string> { "/readonly" } },
			new FakeSelectorStorage(iStorage), new FakeIWebLogger());
	}

	[TestMethod]
	public async Task ReplaceServiceTest_NotFound()
	{
		var output = await _metaReplace.Replace("/not-found.jpg",
			nameof(FileIndexItem.Tags),
			TrashKeyword.TrashKeywordString, string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, output[0].Status);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_NotFoundOnDiskButFoundInDatabase()
	{
		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "only-found-in-db.jpg", ParentDirectory = "/", Tags = "test1, test"
		});

		var output = await _metaReplace.Replace("/only-found-in-db.jpg",
			nameof(FileIndexItem.Tags),
			TrashKeyword.TrashKeywordString, string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, output[0].Status);

		await _query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_ToDeleteStatus()
	{
		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test2.jpg", ParentDirectory = "/", Tags = "test1, test"
		});

		var output = await _metaReplace.Replace("/test2.jpg",
			nameof(FileIndexItem.Tags), "test1",
			TrashKeyword.TrashKeywordString, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, output[0].Status);
		Assert.AreEqual($"{TrashKeyword.TrashKeywordString}, test", output[0].Tags);

		await _query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceString_OkStatus()
	{
		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test2.jpg",
			ParentDirectory = "/",
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test",
			Status = FileIndexItem.ExifStatus.Ok
		});

		var output = await _metaReplace.Replace("/test2.jpg",
			nameof(FileIndexItem.Tags),
			TrashKeyword.TrashKeywordString, string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);
		Assert.AreEqual("test1, test", output[0].Tags);
		await _query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceString_OkAndSameStatus()
	{
		// When an item is cached it OkAndSame

		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test_ok_and_same.jpg",
			ParentDirectory = "/",
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test",
			Status = FileIndexItem.ExifStatus.OkAndSame // this okAndSame if its cached
		});

		var output = await _metaReplace.Replace("/test_ok_and_same.jpg",
			nameof(FileIndexItem.Tags), TrashKeyword.TrashKeywordString,
			string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);
		Assert.AreEqual("test1, test", output[0].Tags);
		await _query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceString_DeletedAndSameStatus()
	{
		// When an item is cached it OkAndSame

		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test_deleted_and_same.jpg",
			ParentDirectory = "/",
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test",
			Status =
				FileIndexItem.ExifStatus.DeletedAndSame // this deletedAndSame if its cached
		});

		var output = await _metaReplace.Replace("/test_deleted_and_same.jpg",
			nameof(FileIndexItem.Tags), TrashKeyword.TrashKeywordString,
			string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);
		Assert.AreEqual("test1, test", output[0].Tags);
		await _query.RemoveItemAsync(item1);
	}


	[TestMethod]
	public void SearchAndReplace_Nothing()
	{
		var result = MetaReplaceService.SearchAndReplace(
			new List<FileIndexItem> { new("/test.jpg") { Status = FileIndexItem.ExifStatus.Ok } },
			"tags", "test", string.Empty);

		Assert.AreEqual(string.Empty, result[0].Tags);
	}

	[TestMethod("Location City is null")]
	public void SearchAndReplace_LocationCityNull()
	{
		var result = MetaReplaceService.SearchAndReplace(
			new List<FileIndexItem>
			{
				new("/test.jpg") { Status = FileIndexItem.ExifStatus.Ok, LocationCity = null }
			},
			"LocationCity", "test", string.Empty);

		Assert.AreEqual(string.Empty, result[0].Tags);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceStringMultipleItems()
	{
		var item0 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test.jpg", ParentDirectory = "/", Tags = TrashKeyword.TrashKeywordString
		});

		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test2.jpg",
			ParentDirectory = "/",
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test"
		});

		var output = await _metaReplace.Replace("/test2.jpg;/test.jpg",
			nameof(FileIndexItem.Tags),
			TrashKeyword.TrashKeywordString, string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);

		Assert.AreEqual(string.Empty, output.Find(p => p.FilePath == "/test.jpg")?.Tags);
		Assert.AreEqual("test1, test", output.Find(p => p.FilePath == "/test2.jpg")?.Tags);

		await _query.RemoveItemAsync(item0);
		await _query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceStringMultipleItems_RawSidecarFile()
	{
		var item0 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test5.dng",
			ParentDirectory = "/",
			Tags = TrashKeyword.TrashKeywordString
		});

		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test5.xmp",
			ParentDirectory = "/",
			ImageFormat = ExtensionRolesHelper.ImageFormat.xmp,
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test"
		});

		var output = await _metaReplace.Replace(item0.FilePath!,
			nameof(FileIndexItem.Tags),
			TrashKeyword.TrashKeywordString, string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);

		Assert.AreEqual(string.Empty, output.Find(p => p.FilePath == item0.FilePath)?.Tags);
		Assert.AreEqual("test1, test", output.Find(p => p.FilePath == item1.FilePath)?.Tags);

		await _query.RemoveItemAsync(item0);
		await _query.RemoveItemAsync(item1);
	}


	[TestMethod]
	public async Task ReplaceServiceTest_replaceStringMultipleItemsCollections()
	{
		var item0 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test.jpg", ParentDirectory = "/", Tags = TrashKeyword.TrashKeywordString
		});

		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test.dng", ParentDirectory = "/", Tags = TrashKeyword.TrashKeywordString
		});

		var output = await _metaReplace.Replace("/test.jpg",
			nameof(FileIndexItem.Tags),
			TrashKeyword.TrashKeywordString, string.Empty, true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[1].Status);

		Assert.AreEqual(string.Empty, output.Find(p => p.FilePath == "/test.jpg")?.Tags);
		Assert.AreEqual(string.Empty, output.Find(p => p.FilePath == "/test.dng")?.Tags);

		await _query.RemoveItemAsync(item0);
		await _query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceStringWithNothingNull()
	{
		var item0 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test.jpg", ParentDirectory = "/", Tags = TrashKeyword.TrashKeywordString
		});

		var output = _metaReplace.Replace("/test2.jpg;/test.jpg",
			nameof(FileIndexItem.Tags), TrashKeyword.TrashKeywordString, null!, false);

		await _query.RemoveItemAsync(item0);
		Assert.IsNotNull(output);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceSearchNull()
	{
		// When you search for nothing, there is nothing to replace 
		var output = await _metaReplace.Replace("/nothing.jpg", nameof(FileIndexItem.Tags),
			null!, "test", false);
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, output[0].Status);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replace_LowerCaseTagName()
	{
		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test2.jpg",
			ParentDirectory = "/",
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test"
		});

		var output = await _metaReplace.Replace("/test2.jpg",
			nameof(FileIndexItem.Tags).ToLowerInvariant(),
			TrashKeyword.TrashKeywordString, string.Empty, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);
		Assert.AreEqual("test1, test", output[0].Tags);

		await _query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_Readonly()
	{
		var item0 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test.jpg",
			ParentDirectory = "/readonly",
			Tags = TrashKeyword.TrashKeywordString
		});

		var output = await _metaReplace.Replace("/readonly/test.jpg",
			nameof(FileIndexItem.Tags), TrashKeyword.TrashKeywordString, null!, false);

		Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, output.FirstOrDefault()?.Status);

		await _query.RemoveItemAsync(item0);
	}

	[TestMethod]
	public void SearchAndReplace_ReplaceDeletedTag_Default()
	{
		var items = new List<FileIndexItem>
		{
			new() { Tags = "test, !keyword!", Status = FileIndexItem.ExifStatus.Ok }
		};
		var result = MetaReplaceService.SearchAndReplace(items, "Tags", "!keyword!", "");
		Assert.AreEqual("test", result.FirstOrDefault()?.Tags);
	}

	[TestMethod]
	public void SearchAndReplace_ReplaceDeletedTag_LowerCase()
	{
		var items = new List<FileIndexItem>
		{
			new() { Tags = "test, !keyword!", Status = FileIndexItem.ExifStatus.Ok }
		};
		var result = MetaReplaceService.SearchAndReplace(items,
			"tags", "!keyword!", "");

		Assert.AreEqual("test", result.FirstOrDefault()?.Tags);
	}

	[TestMethod]
	public void SearchAndReplace_ReplaceDeletedTag_StatusDeleted()
	{
		var items = new List<FileIndexItem>
		{
			new()
			{
				Tags = $"test, {TrashKeyword.TrashKeywordString}",
				Status = FileIndexItem.ExifStatus.Deleted
			}
		};
		var result = MetaReplaceService.SearchAndReplace(items,
			"tags", TrashKeyword.TrashKeywordString, "");
		Assert.AreEqual("test", result.FirstOrDefault()?.Tags);
	}

	[TestMethod]
	public async Task ReplaceServiceTest_replaceString_Duplicate_Input()
	{
		var item1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test34598.jpg",
			ParentDirectory = "/",
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test"
		});
		var item2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "test34598.jpg",
			ParentDirectory = "/",
			Tags = $"test1, {TrashKeyword.TrashKeywordString}, test"
		});


		var output = await _metaReplace.Replace("/test34598.jpg",
			nameof(FileIndexItem.Tags), TrashKeyword.TrashKeywordString,
			string.Empty, false);

		await _query.RemoveItemAsync(item1);
		await _query.RemoveItemAsync(item2);

		Assert.AreEqual(1, output.Count);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, output[0].Status);
		Assert.AreEqual("test1, test", output[0].Tags);
	}
}
