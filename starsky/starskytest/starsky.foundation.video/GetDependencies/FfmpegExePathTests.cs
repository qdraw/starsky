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

	[TestMethod]
	public void GetExeParentFolder_ShouldReturnCorrectPath()
	{
		var expectedPath = Path.Combine(_appSettings.DependenciesFolder, "ffmpeg");
		var result = _ffmpegExePath.GetExeParentFolder(string.Empty);
		Assert.AreEqual(expectedPath, result);
	}

	[TestMethod]
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
