using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using starsky.foundation.native.FileSystem.Interfaces;

namespace starskytest.FakeMocks;

[SuppressMessage("Usage", "S1104: Change this field to be read-only, static, or a constant")]
public class FakeMacOsSecurityScopedBookmarkNative : IMacOsSecurityScopedBookmarkNative
{
	// --- Configurable return values ---

	public IntPtr FileUrlToReturn { get; set; } = new(100);
	public IntPtr BookmarkDataToReturn { get; set; } = new(200);
	public IntPtr ResolvedUrlToReturn { get; set; } = new(300);
	public bool StartAccessResult { get; set; } = true;
	public string PathToReturn { get; set; } = "/resolved/path";
	public IntPtr NsDataToReturn { get; set; } = new(400);
	public byte[] NsDataBytesToReturn { get; set; } = [1, 2, 3];

	// --- Call tracking ---

	public int CfReleaseCalls { get; private set; }
	public int ObjcReleaseCalls { get; private set; }

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global",
		Justification = "Read by test assertions")]
	public bool StopAccessCalled { get; private set; }

	[SuppressMessage("ReSharper", "CollectionNeverQueried.Global",
		Justification = "Read by test assertions")]
	[SuppressMessage("Usage",
		"S4004: Make 'AllCfReleased' a read-only collection or provide a setter")]
	public List<IntPtr> AllCfReleased { get; } = [];

	// --- Interface implementation ---

	public virtual IntPtr CreateFileUrl(string path)
	{
		return FileUrlToReturn;
	}

	public virtual IntPtr CreateBookmarkData(IntPtr fileUrl)
	{
		return BookmarkDataToReturn;
	}

	public virtual IntPtr ResolveBookmarkData(IntPtr nsData)
	{
		return ResolvedUrlToReturn;
	}

	public virtual bool StartAccessingSecurityScopedResource(IntPtr url)
	{
		return StartAccessResult;
	}

	public virtual void StopAccessingSecurityScopedResource(IntPtr url)
	{
		StopAccessCalled = true;
	}

	public virtual string GetPath(IntPtr url)
	{
		return PathToReturn;
	}

	public virtual IntPtr NsDataFromBytes(byte[] bytes)
	{
		return NsDataToReturn;
	}

	public virtual byte[] NsDataGetBytes(IntPtr nsData)
	{
		return NsDataBytesToReturn;
	}

	public virtual void CfRelease(IntPtr handle)
	{
		CfReleaseCalls++;
		AllCfReleased.Add(handle);
	}

	public virtual void ObjcRelease(IntPtr obj)
	{
		ObjcReleaseCalls++;
	}
}
