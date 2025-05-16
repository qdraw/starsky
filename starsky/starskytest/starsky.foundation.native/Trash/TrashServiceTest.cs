using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash;
using starsky.foundation.native.Trash.Helpers;
using starsky.foundation.platform.Architecture;
using starskytest.starsky.foundation.platform.Architecture;

namespace starskytest.starsky.foundation.native.Trash;

[TestClass]
public class TrashServiceTest
{
	[TestMethod]
	public void TrashService_CanUseSystemTrash_Compare()
	{
		var result = new TrashService().DetectToUseSystemTrash();
		var internalApi = TrashService.DetectToUseSystemTrashInternal(
			RuntimeInformation.IsOSPlatform,
			Environment.UserInteractive,
			Environment.UserName);
		Assert.AreEqual(internalApi, result);
	}

	[TestMethod]
	public void TrashService_SingleItem_Trash()
	{
		var result = new TrashService().Trash("test");

		// This feature is not working on Linux and FreeBSD
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.Linux ||
		     OperatingSystemHelper.GetPlatform() == OSPlatform.FreeBSD )
		{
			Assert.IsNull(result);
			return;
		}

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void TrashService_List_Trash()
	{
		var result = new TrashService().Trash(new List<string> { "test" });

		// This feature is not working on Linux and FreeBSD
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.Linux ||
		     OperatingSystemHelper.GetPlatform() == OSPlatform.FreeBSD )
		{
			Assert.IsNull(result);
			return;
		}

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DetectToUseSystemTrashInternal_Root_MacOs()
	{
		var result =
			TrashService.DetectToUseSystemTrashInternal(FakeOsOverwrite.IsMacOs,
				true, "root");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void DetectToUseSystemTrashInternal_InteractiveFalse_MacOs()
	{
		var result =
			TrashService.DetectToUseSystemTrashInternal(FakeOsOverwrite.IsMacOs,
				false, "test");
		Assert.IsFalse(result);
	}


	[TestMethod]
	public void DetectToUseSystemTrashInternal_Linux()
	{
		var result =
			TrashService.DetectToUseSystemTrashInternal(FakeOsOverwrite.IsLinux,
				true, "test");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void DetectToUseSystemTrashInternal_Windows_AsWindowsService_InteractiveFalse()
	{
		var result =
			TrashService.DetectToUseSystemTrashInternal(FakeOsOverwrite.IsWindows,
				false, "test");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void DetectToUseSystemTrashInternal_Windows_User()
	{
		var (driveHasBin, _, _) = WindowsShellTrashBindingHelper.DriveHasRecycleBin();

		var result =
			TrashService.DetectToUseSystemTrashInternal(FakeOsOverwrite.IsWindows,
				true, "test");

		// output should be the same, different as WindowsShellTrashBindingHelper DriveHasRecycleBin
		Assert.AreEqual(driveHasBin, result);
	}
}
