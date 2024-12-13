using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.video.GetDependencies;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegPrepareBeforeRunningTests
{
	private readonly FfMpegPrepareBeforeRunning _ffMpegPrepareBeforeRunning;
	private readonly FakeIStorage _storage;

	public FfMpegPrepareBeforeRunningTests()
	{
		_storage = new FakeIStorage([], ["/"]);
		_ffMpegPrepareBeforeRunning = new FfMpegPrepareBeforeRunning(
			new FakeSelectorStorage(_storage),
			new FakeIMacCodeSign(_storage),
			new FakeIFfmpegChmod(_storage),
			new AppSettings(),
			new FakeIWebLogger());
	}

	private async Task WriteOrDeleteFile(bool fileExists, string path)
	{
		if ( fileExists )
		{
			await _storage.WriteStreamAsync(StringToStreamHelper.StringToStream("1"), path);
		}
		else
		{
			_storage.FileDelete(path);
		}
	}

	[DataTestMethod]
	[DataRow("win-x64", true, true, true, true)]
	[DataRow("win-x64", true, false, false, true)] // no chmod on win-x64
	[DataRow("win-arm64", true, true, true, true)]
	[DataRow("win-arm64", true, false, false, true)] // no chmod on win-arm,64
	[DataRow("linux-x64", true, true, false, true)] // no macCodeSign on linux-x64
	[DataRow("linux-x64", true, true, true, true)]
	[DataRow("osx-x64", true, true, false, false)]
	[DataRow("osx-x64", true, true, true, true)]
	[DataRow("osx-arm64", true, true, false, false)]
	[DataRow("osx-arm64", true, true, true, true)]
	[DataRow("linux-x64", false, true, true, false)]
	public async Task PrepareBeforeRunning_ShouldReturn(
		string architecture, bool fileExists, bool chmodSuccess, bool macCodeSignSuccess,
		bool expectedResult)
	{
		// Arrange
		var exeFile = new FfmpegExePath(new AppSettings()).GetExePath(architecture);
		await WriteOrDeleteFile(fileExists, exeFile);

		var chmodPath = new FfMpegChmod(_storage, new FakeIWebLogger()).CmdPath;
		await WriteOrDeleteFile(chmodSuccess, chmodPath);

		var macCodeSign = new MacCodeSign(new FakeSelectorStorage(_storage), new FakeIWebLogger())
			.CodeSignPath;
		await WriteOrDeleteFile(macCodeSignSuccess, macCodeSign);

		// Act
		var result = await _ffMpegPrepareBeforeRunning.PrepareBeforeRunning(architecture);

		// Assert
		Assert.AreEqual(expectedResult, result);
	}
}
