using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Architecture;

namespace starskytest.starsky.foundation.platform.Architecture;

[TestClass]
public class CurrentArchitectureTests
{
	[TestMethod]
	[DataRow("Windows", "X86", "win-x86")]
	[DataRow("Windows", "X64", "win-x64")]
	[DataRow("Windows", "Arm", "win-arm")]
	[DataRow("Windows", "Arm64", "win-arm64")]
	[DataRow("Linux", "X86", "linux-x86")]
	[DataRow("Linux", "X64", "linux-x64")]
	[DataRow("Linux", "Arm", "linux-arm")]
	[DataRow("Linux", "Arm64", "linux-arm64")]
	[DataRow("OSX", "X86", "osx-x86")]
	[DataRow("OSX", "X64", "osx-x64")]
	[DataRow("OSX", "Arm", "osx-arm")]
	[DataRow("OSX", "Arm64", "osx-arm64")]
	public void GetCurrentRuntimeIdentifier_ShouldReturnExpectedIdentifier(string osPlatformString,
		string architectureString, string expected)
	{
		// Arrange
		var osPlatform = OSPlatform.Create(osPlatformString);
		var architecture =
			Enum.Parse<System.Runtime.InteropServices.Architecture>(architectureString);

		// Act
		var result =
			CurrentArchitecture.GetCurrentRuntimeIdentifier(IsOsPlatformDelegate,
				OsArchitectureDelegate);

		// Assert
		Assert.AreEqual(expected, result);

		bool IsOsPlatformDelegate(OSPlatform platform)
		{
			return platform == osPlatform;
		}

		System.Runtime.InteropServices.Architecture OsArchitectureDelegate()
		{
			return architecture;
		}
	}

	[TestMethod]
	public void GetCurrentRuntimeIdentifier_CurrentOs()
	{
		var result = CurrentArchitecture.GetCurrentRuntimeIdentifier();

		Assert.IsNotNull(result);

		Assert.EndsWith(RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), result);

		var platform = OperatingSystemHelper.GetPlatform();

		if ( platform == OSPlatform.Windows )
		{
			Assert.StartsWith("win", result);
		}
		else if ( platform == OSPlatform.Linux || platform == OSPlatform.FreeBSD )
		{
			Assert.StartsWith("linux", result);
		}
		else if ( platform == OSPlatform.OSX )
		{
			Assert.StartsWith("osx", result);
		}
		else
		{
			Assert.StartsWith("unknown", result);
		}
	}
}
