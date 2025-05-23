using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
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
	private readonly FakeIWebLogger _logger;
	private readonly MacCodeSign _macCodeSign;
	private readonly string _testFolder;

	public MacCodeSignTests()
	{
		_logger = new FakeIWebLogger();
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(_logger);
		_macCodeSign = new MacCodeSign(new FakeSelectorStorage(_hostFileSystemStorage), _logger);
		_testFolder = Path.Combine(new CreateAnImage().BasePath, "MacCodeSignTests");
		CreateTestFolder();
	}

	private void CreateTestFolder()
	{
		if ( Directory.Exists(_testFolder) )
		{
			return;
		}

		Directory.CreateDirectory(_testFolder);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( !Directory.Exists(_testFolder) )
		{
			return;
		}

		Directory.Delete(_testFolder, true);
	}

	private async Task<string> CreateStubExeFile()
	{
		var chmodHelper = new FfMpegChmod(new FakeSelectorStorage(_hostFileSystemStorage),
			new FakeIWebLogger());
		var exeFile = Path.Combine(_testFolder, "testExecutable");
		CreateStubFile(exeFile, "#!/bin/bash\necho Fake Executable");
		await chmodHelper.Chmod(exeFile);
		return exeFile;
	}

	private async Task CreateStubCodeSignFile(int codeSignExitCode)
	{
		var chmodHelper = new FfMpegChmod(new FakeSelectorStorage(_hostFileSystemStorage),
			new FakeIWebLogger());
		var codeSignPath = Path.Combine(_testFolder, "codesign");
		CreateStubFile(codeSignPath, $"#!/bin/bash\necho codesign\nexit {codeSignExitCode}");
		_macCodeSign.CodeSignPath = codeSignPath;
		await chmodHelper.Chmod(codeSignPath);
	}

	private async Task CreateStubXattrFile(int xattrExitCode)
	{
		var chmodHelper = new FfMpegChmod(new FakeSelectorStorage(_hostFileSystemStorage),
			new FakeIWebLogger());
		var xattrPath = Path.Combine(_testFolder, "xattr");
		CreateStubFile(xattrPath, $"#!/bin/bash\nexit {xattrExitCode}");
		_macCodeSign.XattrPath = xattrPath;
		await chmodHelper.Chmod(xattrPath);
	}

	private async Task<string> CreateStubFiles(int codeSignExitCode, int xattrExitCode)
	{
		var exeFile = await CreateStubExeFile();
		await CreateStubCodeSignFile(codeSignExitCode);
		await CreateStubXattrFile(xattrExitCode);
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
	public async Task MacCodeSignAndXattrExecutable_Status_ShouldReturnExpectedResult__UnixOnly(
		int codeSignExitCode,
		int xattrExitCode, bool expectedResult)
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		// Arrange
		var exeFile = await CreateStubFiles(codeSignExitCode, xattrExitCode);

		// Act
		var result = await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile);

		// Assert
		Assert.AreEqual(expectedResult, result);
	}

	[TestMethod]
	[DataRow(false, false, null)]
	[DataRow(true, false, null)]
	[DataRow(false, true, null)]
	[DataRow(true, true, true)]
	public async Task MacCodeSignAndXattrExecutable_NotFound_ShouldReturnExpectedResult__UnixOnly(
		bool codeSignExists,
		bool xattrExists, bool? expectedResult)
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		Cleanup();
		CreateTestFolder();

		// Arrange
		if ( xattrExists )
		{
			await CreateStubXattrFile(0);
		}
		else
		{
			_macCodeSign.XattrPath = Path.Combine(_testFolder, "xattr_not_found");
		}

		if ( codeSignExists )
		{
			await CreateStubCodeSignFile(0);
		}
		else
		{
			_macCodeSign.CodeSignPath = Path.Combine(_testFolder, "codesign_not_found");
		}

		var exeFile = await CreateStubExeFile();

		// Act
		var result = await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile);

		// Assert
		Assert.AreEqual(expectedResult, result);
	}

	[TestMethod]
	public async Task MacCodeSignExecutable_ShouldLogError_WhenCodeSignFails__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		// Arrange
		var exeFile = await CreateStubFiles(1, 0);

		// Act
		var result = await _macCodeSign.MacCodeSignExecutable(exeFile);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(_logger.TrackedExceptions.Exists(entry =>
			entry.Item2?.Contains("codesign Command failed with exit code") == true));
	}

	[TestMethod]
	public async Task MacXattrExecutable_ShouldLogError_WhenXattrFails__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		// Arrange
		var exeFile = await CreateStubFiles(0, 1);

		// Act
		var result = await _macCodeSign.MacXattrExecutable(exeFile);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(_logger.TrackedExceptions.Exists(entry =>
			entry.Item2?.Contains("xattr Command failed with exit code") == true));
	}

	[TestMethod]
	public async Task MacCodeSignAndXattrExecutable_QuarantineEventsV2_ShouldReturnTrue__MacOnly()
	{
		if ( !OperatingSystem.IsMacOS() )
		{
			Assert.Inconclusive("This test is only applicable on macOS.");
			return;
		}

		// Arrange
		var exeFile = await CreateStubExeFile();

		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		var addResult = await Command.Run("sqlite3",
			$"{home}/Library/Preferences/com.apple.LaunchServices.QuarantineEventsV2",
			"INSERT INTO LSQuarantineEvent (LSQuarantineEventIdentifier, LSQuarantineTimeStamp, " +
			"LSQuarantineAgentBundleIdentifier, LSQuarantineAgentName, LSQuarantineDataURLString, " +
			"LSQuarantineSenderName, LSQuarantineSenderAddress, LSQuarantineTypeNumber, " +
			"LSQuarantineOriginTitle, LSQuarantineOriginURLString, LSQuarantineOriginAlias) VALUES " +
			"('B555DB5F-D82A-408B-B9A6-D4F4012FD520', 570726604.559004, 'com.apple.Safari', 'Safari', " +
			"'https://qdraw.nl/test.zip', NULL, NULL, 0, NULL, " +
			" 'https://github.com/qdraw/starsky/releases', NULL);"
		).Task;

		Console.WriteLine(addResult.StandardOutput);
		Console.WriteLine(addResult.StandardError);

		await Command.Run("xattr", "-w", "com.apple.quarantine",
			"\"0081;5f8e2e1e;Safari;B555DB5F-D82A-408B-B9A6-D4F4012FD520\"",
			exeFile).Task;

		var xattrBefore = await Command.Run("xattr", "-p",
			"com.apple.quarantine", exeFile).Task;

		Console.WriteLine("xattr list:");
		Console.WriteLine(xattrBefore.StandardOutput);

		Assert.AreEqual("\"0081;5f8e2e1e;Safari;B555DB5F-D82A-408B-B9A6-D4F4012FD520\"",
			xattrBefore.StandardOutput.Trim());

		// Act
		var result = await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile);

		var xattrAfter = await Command.Run("xattr", "-p",
			"com.apple.quarantine", exeFile).Task;

		// Assert
		Assert.AreEqual(string.Empty, xattrAfter.StandardOutput);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task MacCodeSignAndXattrExecutable_CodeSign__MacOnly()
	{
		if ( !OperatingSystem.IsMacOS() )
		{
			Assert.Inconclusive("This test is only applicable on macOS.");
			return;
		}

		// Arrange
		var exeFile = await CreateStubExeFile();

		var codeSignBefore = await Command.Run("codesign", "-dvv", exeFile).Task;

		// Act
		var result = await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile);

		var codeSignAfter = await Command.Run("codesign", "-dvv", exeFile).Task;
		Console.WriteLine(codeSignAfter.StandardOutput);
		Console.WriteLine(codeSignAfter.StandardError);

		// Assert
		Assert.IsTrue(result);
		Assert.IsTrue(codeSignBefore.StandardError.Contains("code object is not signed at all"));
		Assert.IsTrue(codeSignAfter.StandardError.Contains("Identifier=testExecutable"));
	}

	[TestMethod]
	[DataRow(nameof(MacCodeSign.MacXattrExecutable))]
	[DataRow(nameof(MacCodeSign.MacCodeSignAndXattrExecutable))]
	[DataRow(nameof(MacCodeSign.MacCodeSignExecutable))]
	[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
	public async Task NotFound(string serviceName)
	{
		bool? result = true;
		switch ( serviceName )
		{
			case nameof(MacCodeSign.MacXattrExecutable):
				result = await _macCodeSign.MacXattrExecutable("not-found");
				break;
			case nameof(MacCodeSign.MacCodeSignAndXattrExecutable):
				result = await _macCodeSign.MacCodeSignAndXattrExecutable("not-found");
				break;
			case nameof(MacCodeSign.MacCodeSignExecutable):
				result = await _macCodeSign.MacCodeSignExecutable("not-found");
				break;
		}

		Assert.IsNull(result);
	}
}
