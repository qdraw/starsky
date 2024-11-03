using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services;

[TestClass]
public sealed class ThumbnailCleanerTest
{
	private readonly Query _query;

	public ThumbnailCleanerTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();

		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase("test");
		var options = builder.Options;
		var context = new ApplicationDbContext(options);
		_query = new Query(context, new AppSettings(), null, null!, memoryCache);
	}

	[TestMethod]
	public async Task ThumbnailCleanerTestAsync_DirectoryNotFoundException()
	{
		var sut = new ThumbnailCleaner(new FakeIStorage(), _query, new FakeIWebLogger(),
			new FakeIThumbnailQuery());

		await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(async () =>
			await sut.CleanAllUnusedFilesAsync());
	}

	[TestMethod]
	public async Task ThumbnailCleanerTestAsync_Cleaner()
	{
		var createAnImage = new CreateAnImage();

		var existFullDir = createAnImage.BasePath + Path.DirectorySeparatorChar + "thumb";
		if ( !Directory.Exists(existFullDir) )
		{
			Directory.CreateDirectory(existFullDir);
		}

		if ( !File.Exists(Path.Join(existFullDir, "EXIST.jpg")) )
		{
			File.Copy(createAnImage.FullFilePath,
				Path.Join(existFullDir, "EXIST.jpg"));
		}

		if ( !File.Exists(Path.Join(existFullDir, "DELETE.jpg")) )
		{
			File.Copy(createAnImage.FullFilePath,
				Path.Join(existFullDir, "DELETE.jpg"));
		}

		await _query.AddItemAsync(new FileIndexItem { FileHash = "EXIST", FileName = "exst2" });

		var appSettings = new AppSettings { ThumbnailTempFolder = existFullDir, Verbose = true };
		var thumbnailStorage =
			new StorageThumbnailFilesystem(appSettings, new FakeIWebLogger());

		var thumbnailCleaner = new ThumbnailCleaner(thumbnailStorage, _query,
			new FakeIWebLogger(), new FakeIThumbnailQuery());

		// there are now two files inside this dir
		var allThumbnailFilesBefore = thumbnailStorage.GetAllFilesInDirectory("/");
		Assert.AreEqual(2, allThumbnailFilesBefore.Count());

		await thumbnailCleaner.CleanAllUnusedFilesAsync();

		// DELETE.jpg is removed > is missing in database
		var allThumbnailFilesAfter = thumbnailStorage.GetAllFilesInDirectory("/");
		Assert.AreEqual(1, allThumbnailFilesAfter.Count());

		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FolderDelete(existFullDir);
	}


	[TestMethod]
	public async Task ThumbnailCleanerTestAsync_CatchException()
	{
		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				// set hash
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large)
			});

		var fakeQuery =
			new FakeIQueryException(
				new RetryLimitExceededException());

		var thumbnailCleaner = new ThumbnailCleaner(fakeStorage, fakeQuery,
			new FakeIWebLogger(), new FakeIThumbnailQuery());

		await thumbnailCleaner.CleanAllUnusedFilesAsync();

		// the file is there even the connection is crashed
		Assert.IsTrue(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large)));
	}

	[TestMethod]
	public async Task ThumbnailCleanerTestAsync_Cleaner_WithDifferentSizes()
	{
		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large),
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.ExtraLarge),
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.ExtraLarge),
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.Large),
				ThumbnailNameHelper.Combine("12234456677", ThumbnailSize.ExtraLarge)
			});

		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/test.jpg") { FileHash = "exist" }
		});

		var thumbnailCleaner = new ThumbnailCleaner(fakeStorage, fakeQuery,
			new FakeIWebLogger(), new FakeIThumbnailQuery());

		await thumbnailCleaner.CleanAllUnusedFilesAsync(1);

		Assert.IsTrue(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta)));
		Assert.IsTrue(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("exist", ThumbnailSize.ExtraLarge)));
		Assert.IsTrue(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("exist", ThumbnailSize.Large)));
		Assert.IsTrue(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta)));

		Assert.IsFalse(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.TinyMeta)));
		Assert.IsFalse(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.ExtraLarge)));
		Assert.IsFalse(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large)));
		Assert.IsFalse(fakeStorage.ExistFile(
			ThumbnailNameHelper.Combine("12234456677", ThumbnailSize.ExtraLarge)));
	}

	[TestMethod]
	public async Task ThumbnailCleanerTestAsync_RemoveFromThumbnailTable()
	{
		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				// set hash
				ThumbnailNameHelper.Combine("35874453877", ThumbnailSize.Large)
			});

		var fakeQuery = new FakeIQuery();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("35874453877", null, null, true, null)
		});

		var preGetter = await thumbnailQuery.Get("35874453877");
		Assert.AreEqual(1, preGetter.Count);

		var thumbnailCleaner = new ThumbnailCleaner(fakeStorage, fakeQuery,
			new FakeIWebLogger(), thumbnailQuery);

		await thumbnailCleaner.CleanAllUnusedFilesAsync();

		var getter = await thumbnailQuery.Get("35874453877");
		Assert.AreEqual(0, getter.Count);
	}

	[DataTestMethod]
	[DataRow("filehash@size", "filehash")]
	[DataRow("filehash", "filehash")]
	[DataRow("", "")]
	[DataRow("filehash@", "filehash")]
	[DataRow("@filehash", "")]
	public void GetFileHashWithoutSize_ReturnsExpectedResult(string input, string expected)
	{
		var result = ThumbnailCleaner.GetFileHashWithoutSize(input);
		Assert.AreEqual(expected, result);
	}
}
