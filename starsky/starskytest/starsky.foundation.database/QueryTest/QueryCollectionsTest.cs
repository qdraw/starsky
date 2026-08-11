using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryCollectionsTest
{
	[TestMethod]
	public void QueryCollections_StackCollections_1()
	{
		var input = new List<FileIndexItem> { new("/test.jpg"), new("/test.dng") };
		var result = Query.StackCollections(input);

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.jpg", result[0].FilePath);
	}

	[TestMethod]
	public void QueryCollections_StackCollections_2()
	{
		var input = new List<FileIndexItem> { new("/test.jpg"), new("/test.mp4") };
		var result = Query.StackCollections(input);

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.jpg", result[0].FilePath);
	}

	[TestMethod]
	public void QueryCollections_StackCollections_NoThumbnailSupported_FallsBackToFirst()
	{
		// Both ARW and DNG are RAW — neither is IsExtensionImageSharpThumbnailSupported.
		// Previously the whole group was silently dropped; now the alphabetically first is kept.
		var input = new List<FileIndexItem> { new("/test.arw"), new("/test.dng") };
		var result = Query.StackCollections(input);

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.arw", result[0].FilePath);
	}

	[TestMethod]
	public void QueryCollections_StackCollections_SingleRaw_PassesThrough()
	{
		// A lone RAW file (no collection partner) should always pass through unchanged.
		var input = new List<FileIndexItem> { new("/test.arw") };
		var result = Query.StackCollections(input);

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.arw", result[0].FilePath);
	}
}
