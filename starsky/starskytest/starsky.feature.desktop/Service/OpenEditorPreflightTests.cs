using System.Collections.Generic;
using System.Threading.Tasks;
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
	public async Task PreflightAsync_NoAppPath()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test.jpg") });
		var appSettingsStub = new AppSettings();
		var storageStub = new FakeIStorage(new List<string>(),
			new List<string> { "/test.jpg" });

		var inputFilePaths = new List<string> { "/test.jpg" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		var result = await openEditorPreflight.PreflightAsync(inputFilePaths, collections);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/test.jpg", result[0].SubPath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result[0].ImageFormat);
		Assert.IsTrue(result[0].FullFilePath.EndsWith("test.jpg"));
		Assert.AreEqual(string.Empty, result[0].AppPath);
	}

	[TestMethod]
	public async Task PreflightAsync_AppPathSet_ButNotFound()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem>
		{
			new FileIndexItem("/test.jpg")
			{
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		});
		var appSettingsStub = new AppSettings
		{
			DefaultDesktopEditor = new List<AppSettingsDefaultEditorApplication>
			{
				new AppSettingsDefaultEditorApplication
				{
					ApplicationPath = "/app/test",
					ImageFormats = new List<ExtensionRolesHelper.ImageFormat>
					{
						ExtensionRolesHelper.ImageFormat.jpg
					}
				}
			}
		};
		var storageStub = new FakeIStorage(new List<string>(),
			new List<string> { "/test.jpg" });

		var inputFilePaths = new List<string> { "/test.jpg" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		var result = await openEditorPreflight.PreflightAsync(inputFilePaths, collections);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/test.jpg", result[0].SubPath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, result[0].ImageFormat);
		Assert.IsTrue(result[0].FullFilePath.EndsWith("test.jpg"));
		Assert.AreEqual(string.Empty, result[0].AppPath);
	}

	[TestMethod]
	public async Task PreflightAsync_AppPathSet()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem>
		{
			new FileIndexItem("/test.jpg")
			{
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		});
		var appSettingsStub = new AppSettings
		{
			DefaultDesktopEditor = new List<AppSettingsDefaultEditorApplication>
			{
				new AppSettingsDefaultEditorApplication
				{
					ApplicationPath = "/app/test",
					ImageFormats = new List<ExtensionRolesHelper.ImageFormat>
					{
						ExtensionRolesHelper.ImageFormat.jpg
					}
				}
			}
		};

		// set a folder in the storage for app path location
		var storageStub = new FakeIStorage(new List<string> { "/app/test" },
			new List<string> { "/test.jpg" });

		var inputFilePaths = new List<string> { "/test.jpg" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		var result = await openEditorPreflight.PreflightAsync(inputFilePaths, collections);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/test.jpg", result[0].SubPath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, result[0].ImageFormat);
		Assert.IsTrue(result[0].FullFilePath.EndsWith("test.jpg"));
		Assert.AreEqual("/app/test", result[0].AppPath);
	}


	[TestMethod]
	public async Task GetObjectsToOpenFromDatabase_NotFound()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test.jpg") });
		var appSettingsStub = new AppSettings();
		var storageStub = new FakeIStorage();

		// Assuming you have appropriate setup for your test case
		var inputFilePaths = new List<string> { "/test.jpg" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		// Act
		var result =
			await openEditorPreflight.GetObjectsToOpenFromDatabase(inputFilePaths, collections);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/test.jpg", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result[0].Status);
	}

	[TestMethod]
	public async Task GetObjectsToOpenFromDatabase_ReadOnly()
	{
		// Arrange
		var queryStub =
			new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/readonly/test.jpg") });
		var appSettingsStub = new AppSettings
		{
			ReadOnlyFolders = new List<string> { "/readonly" }
		};
		var storageStub =
			new FakeIStorage(new List<string>(),
				new List<string> { "/readonly/test.jpg" });

		// Assuming you have appropriate setup for your test case
		var inputFilePaths = new List<string> { "/readonly/test.jpg" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		// Act
		var result =
			await openEditorPreflight.GetObjectsToOpenFromDatabase(inputFilePaths, collections);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/readonly/test.jpg", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, result[0].Status);
	}

	[TestMethod]
	public async Task GetObjectsToOpenFromDatabase_SkipXmpSidecar()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem>
		{
			new FileIndexItem("/test.xmp")
			{
				ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
			}
		});
		var appSettingsStub = new AppSettings();
		var storageStub = new FakeIStorage(new List<string>(),
			new List<string> { "/test.xmp" });

		// Assuming you have appropriate setup for your test case
		var inputFilePaths = new List<string> { "/test.xmp" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		// Act
		var result =
			await openEditorPreflight.GetObjectsToOpenFromDatabase(inputFilePaths, collections);

		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public async Task GetObjectsToOpenFromDatabase_ChangeDefaultToOkStatus()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem>
		{
			new FileIndexItem("/test.mp4")
			{
				ImageFormat = ExtensionRolesHelper.ImageFormat.mp4,
				Status = FileIndexItem.ExifStatus.Default // difference here!
			}
		});
		var appSettingsStub = new AppSettings();
		var storageStub = new FakeIStorage(new List<string>(),
			new List<string> { "/test.mp4" });

		// Assuming you have appropriate setup for your test case
		var inputFilePaths = new List<string> { "/test.mp4" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		// Act
		var result =
			await openEditorPreflight.GetObjectsToOpenFromDatabase(inputFilePaths, collections);

		// Change the status to Ok
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/test.mp4", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
	}

	[TestMethod]
	public async Task GetObjectsToOpenFromDatabase_Duplicates()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem>
		{
			new FileIndexItem("/test.mp4")
			{
				ImageFormat = ExtensionRolesHelper.ImageFormat.mp4,
				Status = FileIndexItem.ExifStatus.Ok
			},
			new FileIndexItem("/test.mp4")
			{
				ImageFormat = ExtensionRolesHelper.ImageFormat.mp4,
				Status = FileIndexItem.ExifStatus.Ok // yes duplicates
			}
		});
		var appSettingsStub = new AppSettings();
		var storageStub = new FakeIStorage(new List<string>(),
			new List<string> { "/test.mp4" });

		// Assuming you have appropriate setup for your test case
		var inputFilePaths = new List<string> { "/test.mp4" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		// Act
		var result =
			await openEditorPreflight.GetObjectsToOpenFromDatabase(inputFilePaths, collections);

		// removed duplicates
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/test.mp4", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
	}

	[TestMethod]
	public async Task GetObjectsToOpenFromDatabase_ChangeOkAndSameToOkStatus()
	{
		// Arrange
		var queryStub = new FakeIQuery(new List<FileIndexItem>
		{
			new FileIndexItem("/test.mp4")
			{
				ImageFormat = ExtensionRolesHelper.ImageFormat.mp4,
				Status = FileIndexItem.ExifStatus.OkAndSame // difference here!
			}
		});
		var appSettingsStub = new AppSettings();
		var storageStub = new FakeIStorage(new List<string>(),
			new List<string> { "/test.mp4" });

		// Assuming you have appropriate setup for your test case
		var inputFilePaths = new List<string> { "/test.mp4" };
		const bool collections = false;

		var openEditorPreflight = new OpenEditorPreflight(queryStub, appSettingsStub,
			new FakeSelectorStorage(storageStub), new FakeIWebLogger());

		// Act
		var result =
			await openEditorPreflight.GetObjectsToOpenFromDatabase(inputFilePaths, collections);

		// Change the status to Ok
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/test.mp4", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
	}

	[TestMethod]
	public void GroupByFileCollectionName_ReturnsCorrectList_WhenAppSettingsIsDefault()
	{
		// Arrange
		var query = new FakeIQuery(); // You can mock IQuery if needed
		var appSettings =
			new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Default };
		var iStorage = new FakeIStorage();
		var preflight =
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage),
				new FakeIWebLogger());

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

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, collection1?.ImageFormat);
	}

	[TestMethod]
	public void GroupByFileCollectionName_ReturnsCorrectList_WhenAppSettingsIsJpeg()
	{
		// Arrange
		var query = new FakeIQuery(); // You can mock IQuery if needed
		var appSettings =
			new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Jpeg };
		var iStorage = new FakeIStorage();
		var preflight =
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage),
				new FakeIWebLogger());

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
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage),
				new FakeIWebLogger());

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
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage),
				new FakeIWebLogger());

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

	[TestMethod]
	public void GroupByFileCollectionName_XmpFile_CollectionsFalse()
	{
		// Arrange
		var query = new FakeIQuery(); // You can mock IQuery if needed
		var appSettings =
			new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Raw };
		var iStorage = new FakeIStorage();
		var preflight =
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage),
				new FakeIWebLogger());

		var fileIndexList = new List<FileIndexItem>
		{
			new FileIndexItem
			{
				FileName = "collection1.xmp",
				ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
			},
			new FileIndexItem
			{
				FileName = "collection1.jpg",
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		// Act 
		// Collection is disabled
		var result = preflight.GroupByFileCollectionName(fileIndexList, false);

		// Assert
		Assert.AreEqual(2, result.Count);

		var collection1Xmp = result.Find(p => p.FileName == "collection1.xmp");
		var collection1Jpg = result.Find(p => p.FileName == "collection1.jpg");

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.xmp, collection1Xmp?.ImageFormat);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, collection1Jpg?.ImageFormat);
	}

	[TestMethod]
	public void GroupByFileCollectionName_Duplicates()
	{
		// Arrange
		var query = new FakeIQuery(); // You can mock IQuery if needed
		var appSettings =
			new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Raw };
		var iStorage = new FakeIStorage();
		var preflight =
			new OpenEditorPreflight(query, appSettings, new FakeSelectorStorage(iStorage),
				new FakeIWebLogger());

		var fileIndexList = new List<FileIndexItem>
		{
			new FileIndexItem
			{
				FileName = "collection1.jpg", // duplicate
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new FileIndexItem
			{
				FileName = "collection1.jpg", // duplicate
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		// Act
		var result = preflight.GroupByFileCollectionName(fileIndexList);

		// Assert
		Assert.AreEqual(1, result.Count);

		var collection1 = result.Find(p => p.FileCollectionName == "collection1");

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, collection1?.ImageFormat);
	}
}
