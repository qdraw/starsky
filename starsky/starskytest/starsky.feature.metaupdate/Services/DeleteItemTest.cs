using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Services;

[TestClass]
public sealed class DeleteItemTest
{
	[TestMethod]
	public async Task Delete_FileNotFound_Ignore()
	{
		var selectorStorage = new FakeSelectorStorage(new FakeIStorage());
		var deleteItem = new DeleteItem(new FakeIQuery(), new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/not-found", true);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,
			result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Delete_NotFoundOnDisk_Ignore()
	{
		var selectorStorage = new FakeSelectorStorage(new FakeIStorage());
		var fakeQuery =
			new FakeIQuery([new("/exist-in-db.jpg")]);
		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/exist-in-db.jpg", true);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
			result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Delete_ReadOnly_Ignored()
	{
		var selectorStorage = new FakeSelectorStorage(new FakeIStorage(["/"],
			["/readonly/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }));

		var fakeQuery =
			new FakeIQuery([new("/readonly/test.jpg")]);
		var deleteItem = new DeleteItem(fakeQuery,
			new AppSettings { ReadOnlyFolders = ["/readonly"] },
			selectorStorage);
		var result = await deleteItem.DeleteAsync("/readonly/test.jpg", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly,
			result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Delete_StatusNotDeleted_Ignored()
	{
		var selectorStorage = new FakeSelectorStorage(new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }));

		var fakeQuery =
			new FakeIQuery([new("/test.jpg")]);
		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test.jpg", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
			result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Delete_IsFileRemoved()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test.jpg") { Tags = TrashKeyword.TrashKeywordString }
			]);
		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test.jpg", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			result.FirstOrDefault()?.Status);

		Assert.IsNull(fakeQuery.GetObjectByFilePath("/test.jpg"));
		Assert.IsFalse(storage.ExistFile("/test.jpg"));
	}


	[TestMethod]
	public async Task Delete_IsFileRemoved_WithCollection()
	{
		var storage = new FakeIStorage(["/", "/dir"],
			["/dir/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
					new("/dir") { IsDirectory = true, Tags = TrashKeyword.TrashKeywordString },
					new("/dir/test.jpg") { Tags = TrashKeyword.TrashKeywordString },
					new("/dir/test.dng") { Tags = TrashKeyword.TrashKeywordString }
				]
			);

		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/dir/test.jpg", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			result.FirstOrDefault()?.Status);

		Assert.IsNull(fakeQuery.GetObjectByFilePath("/test.jpg"));
		Assert.IsFalse(storage.ExistFile("/test.jpg"));

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			result[1].Status);

		Assert.IsNull(fakeQuery.GetObjectByFilePath("/test.dng"));
		Assert.IsFalse(storage.ExistFile("/test.dng"));
	}

	[TestMethod]
	public async Task Delete_IsJsonSideCarFileRemoved()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg", "/.starsky.test.jpg.json"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test.jpg") { Tags = TrashKeyword.TrashKeywordString }
			]);
		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test.jpg", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			result.FirstOrDefault()?.Status);

		Assert.IsFalse(storage.ExistFile("/.starsky.test.jpg.json"));
	}

	[TestMethod]
	public async Task Delete_IsXmpSideCarFileRemoved()
	{
		var storage = new FakeIStorage(["/"],
			["/test.dng", "/test.xmp"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test.dng") { Tags = TrashKeyword.TrashKeywordString }
			]);
		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test.dng", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			result.FirstOrDefault()?.Status);

		Assert.IsFalse(storage.ExistFile("/test.xmp"));
	}

	[TestMethod]
	public async Task Delete_IsFolderRemoved()
	{
		var storage = new FakeIStorage(["/test", "/"],
			[],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test") { IsDirectory = true, Tags = TrashKeyword.TrashKeywordString }
			]);

		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			result.FirstOrDefault()?.Status);

		Assert.IsNull(fakeQuery.GetObjectByFilePath("/test"));
		Assert.IsFalse(storage.ExistFolder("/test"));
	}

	[TestMethod]
	public async Task Delete_IsFolderRemoved_IncludingChildFolders()
	{
		var storage = new FakeIStorage(
			["/test", "/", "/test/child_folder"],
			["/test/child_folder/i.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test") { IsDirectory = true, Tags = TrashKeyword.TrashKeywordString },
				new("/test/child_folder") { IsDirectory = true },
				new("/test/child_folder/2") { IsDirectory = true }
			]);
		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test", true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			result.FirstOrDefault()?.Status);

		Assert.IsEmpty(fakeQuery.GetAllFolders());
		Assert.IsNull(fakeQuery.GetObjectByFilePath("/test"));
		Assert.IsNull(fakeQuery.GetObjectByFilePath("/test/child_folder"));
		Assert.IsNull(fakeQuery.GetObjectByFilePath("/test/child_folder/2"));
		Assert.IsFalse(storage.ExistFolder("/test"));
	}

	[TestMethod]
	public async Task Delete_DirectoryWithChildItems_CollectionsOn()
	{
		var storage = new FakeIStorage(["/test", "/"],
			["/test/image.jpg", "/test/image.dng"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test") { IsDirectory = true, Tags = TrashKeyword.TrashKeywordString },
				new("/test/image.jpg"),
				new("/test/image.dng")
			]);

		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test", true);

		Assert.HasCount(3, result);
		Assert.AreEqual("/test", result[0].FilePath);
		Assert.AreEqual("/test/image.jpg", result[1].FilePath);
		Assert.AreEqual("/test/image.dng", result[2].FilePath);

		Assert.IsEmpty(storage.GetAllFilesInDirectoryRecursive("/"));
		Assert.IsEmpty(await fakeQuery.GetAllRecursiveAsync());
	}

	[TestMethod]
	public async Task Delete_DirectoryWithChildItems_CollectionsOff()
	{
		var storage = new FakeIStorage(["/test", "/"],
			["/test/image.jpg", "/test/image.dng"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test") { IsDirectory = true, Tags = TrashKeyword.TrashKeywordString },
				new("/test/image.jpg"),
				new("/test/image.dng")
			]);

		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test", false);

		Assert.HasCount(3, result);
		Assert.AreEqual("/test", result[0].FilePath);
		Assert.AreEqual("/test/image.jpg", result[1].FilePath);
		Assert.AreEqual("/test/image.dng", result[2].FilePath);

		Assert.IsEmpty(storage.GetAllFilesInDirectoryRecursive("/"));
		Assert.IsEmpty(await fakeQuery.GetAllRecursiveAsync());
	}


	[TestMethod]
	public async Task Delete_ChildDirectories()
	{
		var storage = new FakeIStorage(
			["/test", "/", "/test/child", "/test/child/child"],
			[],
			new List<byte[]>());
		var selectorStorage = new FakeSelectorStorage(storage);

		var fakeQuery =
			new FakeIQuery([
				new("/test") { IsDirectory = true, Tags = TrashKeyword.TrashKeywordString },
				new("/test/child") { IsDirectory = true },
				new("/test/child/child") { IsDirectory = true }
			]);

		var deleteItem = new DeleteItem(fakeQuery, new AppSettings(), selectorStorage);
		var result = await deleteItem.DeleteAsync("/test", false);

		Assert.HasCount(3, result);
		Assert.AreEqual("/test", result[0].FilePath);
		Assert.AreEqual("/test/child", result[1].FilePath);
		Assert.AreEqual("/test/child/child", result[2].FilePath);
	}
}
