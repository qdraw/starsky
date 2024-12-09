using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class MacCodeSignTests
{
	private readonly StorageHostFullPathFilesystem _hostFileSystemStorage;
	private readonly MacCodeSign _macCodeSign;
	private readonly string _testFolder;
	private readonly FakeIWebLogger _logger;

	public MacCodeSignTests()
	{
		_logger = new FakeIWebLogger();
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(_logger);
		_macCodeSign = new MacCodeSign(new FakeSelectorStorage(_hostFileSystemStorage), logger);
		_testFolder = Path.Combine(new CreateAnImage().BasePath, "MacCodeSignTests");
		Directory.CreateDirectory(_testFolder);
	}

	[TestCleanup]
	public void Cleanup()
	{
		Directory.Delete(_testFolder, true);
	}

	private async Task<string> CreateSubFiles(int codeSignExitCode, int xattrExitCode)
	{
		var chmodHelper = new FfmpegChmod(_hostFileSystemStorage, new FakeIWebLogger());

		var exeFile = Path.Combine(_testFolder, "testExecutable");
		CreateStubFile(exeFile, "#!/bin/bash\necho Fake Executable");


		var codeSignPath = Path.Combine(_testFolder, "codesign");
		CreateStubFile(codeSignPath, $"#!/bin/bash\necho codesign\nexit {codeSignExitCode}");
		_macCodeSign.CodeSignPath = codeSignPath;

		var xattrPath = Path.Combine(_testFolder, "xattr");
		CreateStubFile(xattrPath, $"#!/bin/bash\nexit {xattrExitCode}");
		_macCodeSign.XattrPath = xattrPath;

		await chmodHelper.Chmod(exeFile);
		await chmodHelper.Chmod(codeSignPath);
		await chmodHelper.Chmod(xattrPath);

		return exeFile;
	}

	private void CreateStubFile(string path, string content)
	{
		var stream = StringToStreamHelper.StringToStream(content);
		_hostFileSystemStorage.WriteStream(stream, path);
	}

	[TestMethod]
	[DataRow(0, 0, true)]
	[DataRow(1, 0, false)]
	[DataRow(0, 1, false)]
	[DataRow(1, 1, false)]
	public async Task MacCodeSignAndXattrExecutable_ShouldReturnExpectedResult(int codeSignExitCode,
		int xattrExitCode, bool expectedResult)
	{
		// Arrange
		var exeFile = await CreateSubFiles(codeSignExitCode, xattrExitCode);

		// Act
		var result = await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile);

		// Assert
		Assert.AreEqual(expectedResult, result);
	}

	[TestMethod]
	public async Task MacCodeSignExecutable_ShouldLogError_WhenCodeSignFails()
	{
		// Arrange
		var exeFile = await CreateSubFiles(1, 0);

		// Act
		var result = await _macCodeSign.MacCodeSignExecutable(exeFile);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(_logger.TrackedExceptions.Exists(entry =>
			entry.Item2?.Contains("codesign Command failed with exit code") == true));
	}

	[TestMethod]
	public async Task MacXattrExecutable_ShouldLogError_WhenXattrFails()
	{
		// Arrange
		var exeFile = await CreateSubFiles(0, 1);

		// Act
		var result = await _macCodeSign.MacXattrExecutable(exeFile);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(_logger.TrackedExceptions.Exists(entry =>
			entry.Item2?.Contains("xattr Command failed with exit code") == true));
	}
}
