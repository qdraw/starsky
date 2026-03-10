using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starskytest.FakeMocks;
using starsky.foundation.sync.Helpers;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public sealed class HasDiskContentOrExistsHelperTest
{
	[TestMethod]
	public async Task HasDiskContentOrExists_FolderExists_ReturnsTrue()
	{
		var storage = new FakeIStorage(["/myfolder"]);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/myfolder");

		Assert.IsTrue(has);
		Assert.AreEqual("folder exists on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_SubdirectoriesExist_ReturnsTrue()
	{
		// parent folder not present but subdirectory exists
		var storage = new FakeIStorage(["/parent/sub"]);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/parent");

		Assert.IsTrue(has);
		Assert.AreEqual("subdirectories exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_FilesExist_ReturnsTrue()
	{
		var storage = new FakeIStorage(["/"], ["/parent/file.jpg"], new List<byte[]?> { null });
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/parent");

		Assert.IsTrue(has);
		Assert.AreEqual("files exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_RetryFindsFolder_ReturnsTrue()
	{
		var storage = new TransientStorage(folderAppearsOnSecondCall: true);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/transient");

		Assert.IsTrue(has);
		Assert.AreEqual("folder exists on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_RetryFindsSubdirectories_ReturnsTrue()
	{
		var storage = new TransientStorage(subdirsAppearOnSecondCall: true);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/transient");

		Assert.IsTrue(has);
		Assert.AreEqual("subdirectories exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_RetryFindsFiles_ReturnsTrue()
	{
		var storage = new TransientStorage(filesAppearOnSecondCall: true);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/transient");

		Assert.IsTrue(has);
		Assert.AreEqual("files exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_NoContent_ReturnsFalse()
	{
		var storage = new FakeIStorage([], [], new List<byte[]>());
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/nothing");

		Assert.IsFalse(has);
		Assert.AreEqual("folder missing and no content found", reason);
	}

	// TransientStorage toggles responses after the first round
	private sealed class TransientStorage : IStorage
	{
		private int _existFolderCalls;
		private int _getDirCalls;
		private int _getFilesCalls;

		private readonly bool _folderAppearsOnSecondCall;
		private readonly bool _subdirsAppearOnSecondCall;
		private readonly bool _filesAppearOnSecondCall;

		public TransientStorage(bool folderAppearsOnSecondCall = false, bool subdirsAppearOnSecondCall = false, bool filesAppearOnSecondCall = false)
		{
			_folderAppearsOnSecondCall = folderAppearsOnSecondCall;
			_subdirsAppearOnSecondCall = subdirsAppearOnSecondCall;
			_filesAppearOnSecondCall = filesAppearOnSecondCall;
		}

		public bool ExistFile(string path) => false;
		public bool ExistFolder(string path)
		{
			_existFolderCalls++;
			if (_existFolderCalls >= 2 && _folderAppearsOnSecondCall)
			{
				return true;
			}

			return false;
		}

		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path) => FolderOrFileModel.FolderOrFileTypeList.Deleted;
		public void FolderMove(string fromPath, string toPath) => throw new NotImplementedException();
		public bool FileMove(string fromPath, string toPath) => throw new NotImplementedException();
		public void FileCopy(string fromPath, string toPath) => throw new NotImplementedException();
		public bool FileDelete(string path) => throw new NotImplementedException();
		public bool CreateDirectory(string path) => throw new NotImplementedException();
		public bool FolderDelete(string path) => throw new NotImplementedException();

		public IEnumerable<string> GetAllFilesInDirectory(string path)
		{
			_getFilesCalls++;
			if (_getFilesCalls >= 2 && _filesAppearOnSecondCall)
			{
				return new List<string> { path + "/file1.jpg" };
			}

			return new List<string>();
		}

		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path) => new List<string>();

		public IEnumerable<string> GetDirectories(string path) => new List<string>();

		public IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path)
		{
			_getDirCalls++;
			if (_getDirCalls >= 2 && _subdirsAppearOnSecondCall)
			{
				return new List<KeyValuePair<string, DateTime>> { new KeyValuePair<string, DateTime>(path + "/sub", DateTime.Now) };
			}

			return new List<KeyValuePair<string, DateTime>>();
		}

		Stream IStorage.ReadStream(string path, int maxRead)
		{
			return ReadStream(path, maxRead);
		}

		bool IStorage.WriteStream(Stream stream, string path)
		{
			return WriteStream(stream, path);
		}

		bool IStorage.WriteStreamOpenOrCreate(Stream stream, string path)
		{
			return WriteStreamOpenOrCreate(stream, path);
		}

		Task<bool> IStorage.WriteStreamAsync(Stream stream, string path)
		{
			return WriteStreamAsync(stream, path);
		}

		public Stream ReadStream(string path, int maxRead = -1) => Stream.Null;
		public bool WriteStream(Stream stream, string path) => throw new NotImplementedException();
		public bool WriteStreamOpenOrCreate(Stream stream, string path) => throw new NotImplementedException();
		public Task<bool> WriteStreamAsync(Stream stream, string path) => throw new NotImplementedException();
		public StorageInfo Info(string path) => new StorageInfo { IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Deleted };
		public bool IsFileReady(string path) => true;
		public IAsyncEnumerable<string> ReadLinesAsync(string path, System.Threading.CancellationToken cancellationToken) => throw new NotImplementedException();
	}
}
