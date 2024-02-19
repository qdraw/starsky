using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.desktop.Service;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.desktop.Service;

[TestClass]
public class OpenEditorPreflightTests
{
	[TestMethod]
	public void GroupByFileCollectionName_ReturnsCorrectList_WhenAppSettingsIsJpeg()
	{
		// Arrange
		var query = new FakeIQuery(); // You can mock IQuery if needed
		var appSettings =
			new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Jpeg };
		var iStorage = new FakeIStorage();
		var preflight =
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage));

		var fileIndexList = new List<FileIndexItem>
		{
			new FileIndexItem
			{
				FileName = "collection1.jpg",
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new FileIndexItem
			{
				FileName = "collection1.tiff",
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new FileIndexItem
			{
				FileName = "collection2.tiff",
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new FileIndexItem
			{
				FileName = "collection3.gif",
				ImageFormat = ExtensionRolesHelper.ImageFormat.gif
			}
		};

		// Act
		var result = preflight.GroupByFileCollectionName(fileIndexList);

		// Assert
		Assert.AreEqual(3, result.Count);

		var collection1 = result.Find(p => p.FileCollectionName == "collection1");
		var collection2 = result.Find(p => p.FileCollectionName == "collection2");
		var collection3 = result.Find(p => p.FileCollectionName == "collection3");

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, collection1?.ImageFormat);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, collection2?.ImageFormat);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gif, collection3?.ImageFormat);
	}

	[TestMethod]
	public void GroupByFileCollectionName_ReturnsCorrectList_WhenAppSettingsIsRaw()
	{
		// Arrange
		var query = new FakeIQuery(); // You can mock IQuery if needed
		var appSettings =
			new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Raw };
		var iStorage = new FakeIStorage();
		var preflight =
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage));

		var fileIndexList = new List<FileIndexItem>
		{
			new FileIndexItem
			{
				FileName = "collection1.jpg",
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new FileIndexItem
			{
				FileName = "collection1.tiff",
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			}
		};

		// Act
		var result = preflight.GroupByFileCollectionName(fileIndexList);

		// Assert
		Assert.AreEqual(1, result.Count);

		var collection1 = result.Find(p => p.FileCollectionName == "collection1");

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, collection1?.ImageFormat);
	}

	[TestMethod]
	public void GroupByFileCollectionName_ReturnsCorrectList_WhenAppSettingsIsOtherType()
	{
		// Arrange
		var query = new FakeIQuery(); // You can mock IQuery if needed
		var appSettings =
			new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Raw };
		var iStorage = new FakeIStorage();
		var preflight =
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage));

		var fileIndexList = new List<FileIndexItem>
		{
			new FileIndexItem
			{
				FileName = "collection1.mp4",
				ImageFormat = ExtensionRolesHelper.ImageFormat.mp4
			},
			new FileIndexItem
			{
				FileName = "collection1.jpg",
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		// Act
		var result = preflight.GroupByFileCollectionName(fileIndexList);

		// Assert
		Assert.AreEqual(1, result.Count);

		var collection1 = result.Find(p => p.FileCollectionName == "collection1");

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, collection1?.ImageFormat);
	}
}
