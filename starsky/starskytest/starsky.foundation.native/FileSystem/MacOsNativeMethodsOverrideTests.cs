using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;
using starsky.foundation.native.FileSystem;

namespace starskytest.starsky.foundation.native.FileSystem;

[TestClass]
public class MacOsNativeMethodsOverrideTests
{
	[TestMethod]
	[OSCondition(OperatingSystems.Linux)]
	public void GetFileSystem_OverrideIsArm64_ReturnsNonEmpty_OnLinux()
	{
		// Arrange: override IsArm64 using the provided test hook
		var original = GetOriginalIsArm64Value();
		try
		{
			MacOsNativeMethods.SetIsArm64(true);

			// Create helper that pretends to be running on macOS but uses a fake mount table resolver
			var helper = new MacOsFileSystemHelper(
				() => OSPlatform.OSX,
				_ => "ext4",
				null,
				null,
				null);

			// Act
			var fs = helper.GetFileSystem("/");

			// Assert
			Assert.IsFalse(string.IsNullOrWhiteSpace(fs));
		}
		finally
		{
			MacOsNativeMethods.SetIsArm64(original);
		}
	}

	private static bool GetOriginalIsArm64Value()
	{
		// Reflectively read the private field since there's no public getter
		var nmType = typeof(MacOsNativeMethods);
		var field = nmType.GetField("IsArm64", BindingFlags.NonPublic | BindingFlags.Static);
		return field is null
			? RuntimeInformation.ProcessArchitecture ==
			  System.Runtime.InteropServices.Architecture.Arm64
			: ( bool ) field.GetValue(null)!;
	}
}
