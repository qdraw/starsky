using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.mcp.Models;
using starsky.foundation.database.Models;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class McpPhotoTest
{
	[TestMethod]
	[DataRow(0d, 0d, false)]
	[DataRow(52.1d, 0d, true)]
	[DataRow(0d, 4.9d, true)]
	[DataRow(52.1d, 4.9d, true)]
	public void FromItem_Location_OnlyWhenCoordinateSet(double latitude, double longitude,
		bool hasLocation)
	{
		var item = new FileIndexItem("/a.jpg") { Latitude = latitude, Longitude = longitude };

		var photo = McpPhoto.FromItem(item);

		Assert.AreEqual(hasLocation, photo.Latitude != null);
		Assert.AreEqual(hasLocation, photo.Longitude != null);
	}

	[TestMethod]
	public void FromItem_IsDirectory_NullIsFalse()
	{
		Assert.IsTrue(McpPhoto.FromItem(new FileIndexItem("/a") { IsDirectory = true })
			.IsDirectory);
		Assert.IsFalse(McpPhoto.FromItem(new FileIndexItem("/a") { IsDirectory = false })
			.IsDirectory);
		Assert.IsFalse(McpPhoto.FromItem(new FileIndexItem("/a") { IsDirectory = null })
			.IsDirectory);
	}

	[TestMethod]
	public void FromItems_RespectsMax()
	{
		var items = new List<FileIndexItem> { new("/a.jpg"), new("/b.jpg"), new("/c.jpg") };

		Assert.HasCount(3, McpPhoto.FromItems(items));
		Assert.HasCount(2, McpPhoto.FromItems(items, 2));
		Assert.IsEmpty(McpPhoto.FromItems([]));
	}
}
