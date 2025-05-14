using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services;

[TestClass]
public class FullFilePathExistsServiceTests
{
	private readonly AppSettings _appSettings;
	private readonly FakeIStorage _hostStorage;
	private readonly FakeSelectorStorageByType _selectorStorage;
	private readonly FakeIStorage _subPathStorage;
	private readonly FakeIStorage _tempStorage;

	public FullFilePathExistsServiceTests()
	{
		_hostStorage = new FakeIStorage();
		_subPathStorage = new FakeIStorage();
		_tempStorage = new FakeIStorage();
		_selectorStorage =
			new FakeSelectorStorageByType(_subPathStorage,
				null!, _hostStorage, _tempStorage);
		_appSettings = new AppSettings { TempFolder = "/", StorageFolder = "/" };
	}

	[DataTestMethod]
	[DataRow(true, "test-file.jpg", true, false, "",
		DisplayName = "File exists in host storage")]
	[DataRow(true, "/file-remote.jpg", false, true, "filehash.jpg",
		DisplayName = "File copied to temp storage")]
	[DataRow(false, "/non-existent-file.jpg", false, false, "",
		DisplayName = "File does not exist anywhere")]
	public async Task GetFullFilePath(bool isSuccess,
		string subPath, bool existsInHost, bool isTempFile, string expectedFileHash)
	{
		// Arrange
		if ( existsInHost )
		{
			var fullFilePath = Path.Combine(_appSettings.StorageFolder, subPath);
			await _hostStorage.WriteStreamAsync(new MemoryStream([1, 2, 3]),
				fullFilePath);
			await _subPathStorage.WriteStreamAsync(new MemoryStream([1, 2, 3]),
				subPath);
		}
		else if ( isTempFile )
		{
			await _subPathStorage.WriteStreamAsync(new MemoryStream([1, 2, 3]),
				subPath);
		}

		var service = new FullFilePathExistsService(_selectorStorage, _appSettings);

		// Act
		var result = await service.GetFullFilePath(subPath, "filehash");

		// Assert
		var expectedFullPath = isTempFile
			? _appSettings.DatabasePathToTempFolderFilePath(expectedFileHash)
			: _appSettings.DatabasePathToFilePath(subPath);

		Assert.AreEqual(expectedFullPath, result.FullFilePath);
		Assert.AreEqual(isTempFile, result.IsTempFile);
		Assert.AreEqual(expectedFileHash, result.TempFileFileHashWithExtension);
		Assert.AreEqual(isSuccess, result.IsSuccess);

		if ( existsInHost )
		{
			var fullFilePath = Path.Combine(_appSettings.StorageFolder, subPath);
			_hostStorage.FileDelete(fullFilePath);
		}
		else if ( isTempFile )
		{
			_subPathStorage.FileDelete(subPath);
		}
	}
}
