using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller.Helpers;

[TestClass]
public sealed class MacOsFullDiskAccessTest
{
	private static FakeSelectorStorage CreateSelectorWithFiles(bool volumesExists,
		bool getDirectoriesThrows = false)
	{
		// fake storage: /Volumes exists or not, GetDirectories may throw to simulate permission error
		var fake = new FakeIStorage(
			volumesExists ? new List<string> { "/Volumes" } : new List<string>(),
			new List<string>());

		if ( !getDirectoriesThrows )
		{
			return new FakeSelectorStorage(fake);
		}

		// create a storage that throws when GetDirectories is called by throwing via ExistFolder true and then making GetDirectories throw
		var throwing = new ThrowingGetDirectoriesStorage();
		return new FakeSelectorStorage(throwing);

	}

	[TestMethod]
	public void CheckMacOsFullDiskAccessOnStartup_NonMac_ReturnsNull()
	{
		var selector = CreateSelectorWithFiles(false);
		var logger = new FakeIWebLogger();
		var sut = new TestMacOsFullDiskAccess(selector, logger, false,
			() => OSPlatform.Create("Unknown"));
		var result = sut.CheckMacOsFullDiskAccessOnStartup();
		Assert.IsNull(result);
	}

	[TestMethod]
	public void CheckMacOsFullDiskAccessOnStartup_Mac_VolumesReadable_ReturnsTrue()
	{
		var selector = CreateSelectorWithFiles(true);
		var logger = new FakeIWebLogger();
		var sut = new TestMacOsFullDiskAccess(selector, logger, false, () => OSPlatform.OSX);
		var result = sut.CheckMacOsFullDiskAccessOnStartup();
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void
		CheckMacOsFullDiskAccessOnStartup_Mac_VolumesNotReadable_OpenReturnsTrue_ReturnsFalse()
	{
		var selector = CreateSelectorWithFiles(true, true);
		var logger = new FakeIWebLogger();
		var sut = new TestMacOsFullDiskAccess(selector, logger, true, () => OSPlatform.OSX);
		var result = sut.CheckMacOsFullDiskAccessOnStartup();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void
		CheckMacOsFullDiskAccessOnStartup_Mac_VolumesNotReadable_OpenReturnsFalse_ReturnsFalse()
	{
		var selector = CreateSelectorWithFiles(true, true);
		var logger = new FakeIWebLogger();
		var sut = new TestMacOsFullDiskAccess(selector, logger, false, () => OSPlatform.OSX);
		var result = sut.CheckMacOsFullDiskAccessOnStartup();
		Assert.IsFalse(result);
	}

	private class ThrowingGetDirectoriesStorage : IStorage
	{
		private readonly FakeIStorage _inner = new FakeIStorage(
			outputSubPathFolders: new List<string> { "/Volumes" },
			outputSubPathFiles: new List<string>());

		public bool IsFileReady(string path) => _inner.IsFileReady(path);
		public IAsyncEnumerable<string> ReadLinesAsync(string path, CancellationToken cancellationToken) => _inner.ReadLinesAsync(path, cancellationToken);
		public string[] ReadAllLines(string path) => _inner.ReadAllLines(path);
		public bool ExistFile(string path) => _inner.ExistFile(path);
		public bool ExistFolder(string path) => _inner.ExistFolder(path);
		public bool IsFolderEmpty(string path) => _inner.IsFolderEmpty(path);
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path) => _inner.IsFolderOrFile(path);
		public void FolderMove(string fromPath, string toPath) => _inner.FolderMove(fromPath, toPath);
		public bool FileMove(string fromPath, string toPath) => _inner.FileMove(fromPath, toPath);
		public void FileCopy(string fromPath, string toPath) => _inner.FileCopy(fromPath, toPath);
		public bool FileDelete(string path) => _inner.FileDelete(path);
		public bool CreateDirectory(string path) => _inner.CreateDirectory(path);
		public bool FolderDelete(string path) => _inner.FolderDelete(path);
		public IEnumerable<string> GetAllFilesInDirectory(string path) => _inner.GetAllFilesInDirectory(path);
		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path) => _inner.GetAllFilesInDirectoryRecursive(path);
		public IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path) => _inner.GetDirectoryRecursive(path);
		public Stream ReadStream(string path, int maxRead = -1) => _inner.ReadStream(path, maxRead);
		public bool WriteStream(Stream stream, string path) => _inner.WriteStream(stream, path);
		public bool WriteStreamOpenOrCreate(Stream stream, string path) => _inner.WriteStreamOpenOrCreate(stream, path);
		public Task<bool> WriteStreamAsync(Stream stream, string path) => _inner.WriteStreamAsync(stream, path);
		public StorageInfo Info(string path) => _inner.Info(path);

		// Simulate permission error when listing directories
		public IEnumerable<string> GetDirectories(string path)
		{
			throw new UnauthorizedAccessException("no access");
		}
	}

	private class TestMacOsFullDiskAccess(
		ISelectorStorage selector,
		IWebLogger logger,
		bool openReturns,
		Func<OSPlatform>? platformResolver = null)
		: MacOsFullDiskAccess(selector, logger, platformResolver)
	{
		protected override bool OpenFullDiskAccessSettings()
		{
			return openReturns;
		}
	}
}
