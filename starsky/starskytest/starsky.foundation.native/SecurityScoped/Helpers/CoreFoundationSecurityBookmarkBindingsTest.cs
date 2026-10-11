using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.SecurityScoped.Helpers;
using starsky.foundation.platform.Architecture;

namespace starskytest.starsky.foundation.native.SecurityScoped.Helpers;

[TestClass]
public class CoreFoundationSecurityBookmarkBindingsTest
{
	[TestMethod]
	public void ResolveAndStartAccessing_NonMacOsPlatform_ReturnsNull()
	{
		var result = CoreFoundationSecurityBookmarkBindings
			.ResolveAndStartAccessing([0x01, 0x02], OSPlatform.Linux);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void ResolveAndStartAccessing_NonMacOsPlatform_Windows_ReturnsNull()
	{
		var result = CoreFoundationSecurityBookmarkBindings
			.ResolveAndStartAccessing([0x01, 0x02], OSPlatform.Windows);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void StopAccessing_NonMacOsPlatform_DoesNotThrow()
	{
		// Should be a no-op on non-macOS — must not throw
		CoreFoundationSecurityBookmarkBindings.StopAccessing(IntPtr.Zero, OSPlatform.Linux);
		CoreFoundationSecurityBookmarkBindings.StopAccessing(new IntPtr(1), OSPlatform.Windows);
	}

	[TestMethod]
	public void StopAccessing_ZeroPtr_MacOs_DoesNotThrow()
	{
		// IntPtr.Zero guard should prevent any P/Invoke call
		CoreFoundationSecurityBookmarkBindings.StopAccessing(IntPtr.Zero, OSPlatform.OSX);
	}

	[TestMethod]
	public void ResolveAndStartAccessing_InvalidBookmarkData_MacOsOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for macOS only");
			return;
		}

		// Garbage bytes are not a valid bookmark — CFDataCreate may succeed but
		// CFURLCreateByResolvingBookmarkData should return zero.
		var result = CoreFoundationSecurityBookmarkBindings
			.ResolveAndStartAccessing([0xDE, 0xAD, 0xBE, 0xEF], OSPlatform.OSX);

		Assert.IsNotNull(result);
		Assert.AreEqual(IntPtr.Zero, result.Value.cfUrl);
		Assert.IsNull(result.Value.path);
	}

	[TestMethod]
	public void ResolveAndStartAccessing_EmptyBytes_MacOsOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for macOS only");
			return;
		}

		var result = CoreFoundationSecurityBookmarkBindings
			.ResolveAndStartAccessing([], OSPlatform.OSX);

		// CFDataCreate with zero length may return non-zero but resolution fails
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public void CfDataCreate_NonMacOs_ThrowsDllNotFoundException()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-macOS only");
			return;
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			CoreFoundationSecurityBookmarkBindings.CfDataCreate(IntPtr.Zero, [0x01], 1));
	}

	[TestMethod]
	public void CfUrlStartAccessingSecurityScopedResource_NonMacOs_ThrowsDllNotFoundException()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-macOS only");
			return;
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			CoreFoundationSecurityBookmarkBindings
				.CfUrlStartAccessingSecurityScopedResource(IntPtr.Zero));
	}

	[TestMethod]
	public void CfUrlStopAccessingSecurityScopedResource_NonMacOs_ThrowsDllNotFoundException()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-macOS only");
			return;
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			CoreFoundationSecurityBookmarkBindings
				.CfUrlStopAccessingSecurityScopedResource(IntPtr.Zero));
	}

	[TestMethod]
	public void CfUrlCopyFileSystemPath_NonMacOs_ThrowsDllNotFoundException()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-macOS only");
			return;
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			CoreFoundationSecurityBookmarkBindings.CfUrlCopyFileSystemPath(IntPtr.Zero, 0));
	}

	[TestMethod]
	public void CfRelease_NonMacOs_ThrowsDllNotFoundException()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-macOS only");
			return;
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			CoreFoundationSecurityBookmarkBindings.CfRelease(IntPtr.Zero));
	}

	[TestMethod]
	public void CfDataCreate_MacOsOnly_ReturnsNonZeroForValidInput()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for macOS only");
			return;
		}

		byte[] bytes = [0x01, 0x02, 0x03];
		var result =
			CoreFoundationSecurityBookmarkBindings.CfDataCreate(IntPtr.Zero, bytes, bytes.Length);

		Assert.AreNotEqual(IntPtr.Zero, result);
		// Release to avoid leak in test
		CoreFoundationSecurityBookmarkBindings.CfRelease(result);
	}

	[TestMethod]
	public void CfStringGetCString_NonMacOs_ThrowsDllNotFoundException()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-macOS only");
			return;
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			CoreFoundationSecurityBookmarkBindings.CfStringGetCString(
				IntPtr.Zero, new byte[64], 64, 0x08000100u));
	}
}
