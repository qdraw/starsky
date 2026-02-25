using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Helpers;
using starsky.foundation.platform.Models;
using System.IO;

namespace starskytest.starsky.foundation.optimisation.Helpers;

[TestClass]
public class ImageOptimisationExePathTest
{
	[TestMethod]
	public void GetExePath_Windows_ReturnsExeWithExtension()
	{
		var appSettings = new AppSettings { DependenciesFolder = "/deps" };
		var sut = new ImageOptimisationExePath(appSettings);
		var result = sut.GetExePath("mozjpeg", "win-x64");
		Assert.AreEqual(Path.Combine("/deps", "mozjpeg-win-x64", "mozjpeg.exe"), result);
	}

	[TestMethod]
	public void GetExePath_WindowsArm_ReturnsExeWithExtension()
	{
		var appSettings = new AppSettings { DependenciesFolder = "/deps" };
		var sut = new ImageOptimisationExePath(appSettings);
		var result = sut.GetExePath("mozjpeg", "win-arm64");
		Assert.AreEqual(Path.Combine("/deps", "mozjpeg-win-arm64", "mozjpeg.exe"), result);
	}

	[TestMethod]
	public void GetExePath_Linux_ReturnsExeWithoutExtension()
	{
		var appSettings = new AppSettings { DependenciesFolder = "/deps" };
		var sut = new ImageOptimisationExePath(appSettings);
		var result = sut.GetExePath("mozjpeg", "linux-x64");
		Assert.AreEqual(Path.Combine("/deps", "mozjpeg-linux-x64", "mozjpeg"), result);
	}

	[TestMethod]
	public void GetExePath_EmptyArchitecture_ReturnsExeWithoutExtension()
	{
		var appSettings = new AppSettings { DependenciesFolder = "/deps" };
		var sut = new ImageOptimisationExePath(appSettings);
		var result = sut.GetExePath("mozjpeg", "");
		Assert.AreEqual(Path.Combine("/deps", "mozjpeg", "mozjpeg"), result);
	}

	[TestMethod]
	public void GetExeParentFolder_EmptyArchitecture()
	{
		var appSettings = new AppSettings { DependenciesFolder = "/deps" };
		var sut = new ImageOptimisationExePath(appSettings);
		var result = sut.GetExeParentFolder("mozjpeg", "");
		Assert.AreEqual(Path.Combine("/deps", "mozjpeg"), result);
	}

	[TestMethod]
	public void GetExeParentFolder_WithArchitecture()
	{
		var appSettings = new AppSettings { DependenciesFolder = "/deps" };
		var sut = new ImageOptimisationExePath(appSettings);
		var result = sut.GetExeParentFolder("mozjpeg", "linux-x64");
		Assert.AreEqual(Path.Combine("/deps", "mozjpeg-linux-x64"), result);
	}
}
