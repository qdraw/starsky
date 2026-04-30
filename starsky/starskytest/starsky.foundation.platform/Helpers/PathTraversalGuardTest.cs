using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class PathTraversalGuardTest
{
	[TestMethod]
	public void ContainsTraversal_TrueForDotDotSegment()
	{
		Assert.IsTrue(PathTraversalGuard.ContainsTraversal("/a/../b.jpg"));
	}

	[TestMethod]
	public void ContainsTraversal_FalseForNormalPath()
	{
		Assert.IsFalse(PathTraversalGuard.ContainsTraversal("/a/b.jpg"));
	}

	[TestMethod]
	public void ToSafeFullPath_ThrowsWhenEscapingRoot()
	{
		var root = Path.GetTempPath();
		try
		{
			PathTraversalGuard.ToSafeFullPath(root, "/../../etc/passwd");
			Assert.Fail("Expected UnauthorizedAccessException");
		}
		catch ( UnauthorizedAccessException )
		{
			// expected
		}
	}
}


