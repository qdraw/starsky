using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.video.GetDependencies;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfmpegExePathTests
{
	private readonly AppSettings _appSettings;
	private readonly FfmpegExePath _ffmpegExePath;

	public FfmpegExePathTests()
	{
		_appSettings = new AppSettings { DependenciesFolder = "test-dependencies" };
		_ffmpegExePath = new FfmpegExePath(_appSettings);
	}

	[DataTestMethod]
	[DataRow(null, "ffmpeg")]
	[DataRow("", "ffmpeg")]
	[DataRow("win-x64", "ffmpeg-win-x64")]
	[DataRow("win-arm64", "ffmpeg-win-arm64")]
	[DataRow("linux-x64", "ffmpeg-linux-x64")]
	[DataRow("osx-x64", "ffmpeg-osx-x64")]
	public void GetExeParentFolder_CurrentArchitecture(string arch, string expectedFolder)
	{
		var expectedPath = Path.Combine(_appSettings.DependenciesFolder, expectedFolder);
		var result = _ffmpegExePath.GetExeParentFolder(arch);
		Assert.AreEqual(expectedPath, result);
	}

	[DataTestMethod]
	[DataRow("win-x64", "ffmpeg.exe")]
	[DataRow("win-arm64", "ffmpeg.exe")]
	[DataRow("linux-x64", "ffmpeg")]
	[DataRow("osx-x64", "ffmpeg")]
	public void GetExePath_ShouldReturnCorrectPath(string architecture, string expectedFileName)
	{
		var expectedPath =
			Path.Combine(_appSettings.DependenciesFolder, $"ffmpeg-{architecture}",
				expectedFileName);
		var result = _ffmpegExePath.GetExePath(architecture);
		Assert.AreEqual(expectedPath, result);
	}
}
