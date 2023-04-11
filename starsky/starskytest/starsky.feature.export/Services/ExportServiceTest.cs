using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.export.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.export.Services;

[TestClass]
public class ExportServiceTest
{
	[TestMethod]
	public async Task Export_Folder()
	{
		var exportService = new ExportService(new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test")
				{
					IsDirectory = true
				},
				new FileIndexItem("/test/test.jpg")
			}), new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}, new List<string>
			{
				"/test/test.jpg"
			})), new FakeIWebLogger(),
			new FakeIThumbnailService());
		
		var (_,fileIndexResultsList) = await exportService.PreflightAsync(new List<string> { "/test" }.ToArray());
		
		Assert.AreEqual(1, fileIndexResultsList.Count);
	}
	
	[TestMethod]
	public async Task Export_Folder_StackCollection_True()
	{
		var exportService = new ExportService(new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test")
				{
					IsDirectory = true
				},
				new FileIndexItem("/test/test.jpg"),
				new FileIndexItem("/test/test.dng")
			}), new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}, new List<string>
			{
				"/test/test.jpg",
				"/test/test.dng"
			})), new FakeIWebLogger(),
			new FakeIThumbnailService());
		
		var (_,fileIndexResultsList) = await exportService.PreflightAsync(new List<string> { "/test/test.jpg" }.ToArray());
		
		Assert.AreEqual(3, fileIndexResultsList.Count);
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.FilePath == "/test/test.jpg"));
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.FilePath == "/test/test.dng"));
	}
	
		
	[TestMethod]
	public async Task Export_Folder_StackCollection_False()
	{
		var exportService = new ExportService(new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test")
				{
					IsDirectory = true
				},
				new FileIndexItem("/test/test.jpg"),
				new FileIndexItem("/test/test.dng")
			}), new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}, new List<string>
			{
				"/test/test.jpg",
				"/test/test.dng"
			})), new FakeIWebLogger(),
			new FakeIThumbnailService());
		
		var (_,fileIndexResultsList) = await exportService.PreflightAsync(new List<string> { "/test/test.jpg" }.ToArray(), false);
		
		Assert.AreEqual(1, fileIndexResultsList.Count);
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.FilePath == "/test/test.jpg"));
		Assert.AreEqual(0, fileIndexResultsList.Count(p => p.FilePath == "/test/test.dng"));
	}

	[TestMethod]
	public async Task Export_Ignore_Deleted_FolderFile()
	{
		var exportService = new ExportService(new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test")
				{
					IsDirectory = true
				},
				new FileIndexItem("/test/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.Deleted
				}
			}), new AppSettings(),
			// file not included in storage
			new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"})), new FakeIWebLogger(),
			new FakeIThumbnailService());
		
		var (_,fileIndexResultsList) = await exportService.PreflightAsync(new List<string> { "/test" }.ToArray());
		
		Assert.AreEqual(0, fileIndexResultsList.Count);
	}
	
	
	[TestMethod]
	public async Task Export_Ignore_Deleted_Folder()
	{
		var exportService = new ExportService(new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test")
				{
					IsDirectory = true
				},
				new FileIndexItem("/test/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.Deleted
				}
			}), new AppSettings(),
			// file not included in storage
			new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"})), new FakeIWebLogger(),
			new FakeIThumbnailService());
		
		var (_,fileIndexResultsList) = await exportService.PreflightAsync(new List<string> { "/test/test.jpg" }.ToArray());
		
		Assert.AreEqual(0, fileIndexResultsList.Count(p => p.Status == FileIndexItem.ExifStatus.Ok));
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing));
	}

	[TestMethod]
	public async Task ExportService_NotFoundNotInIndex()
	{
		var exportService = new ExportService(new FakeIQuery(), new AppSettings(), new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());
		var (_, fileIndexResultsList) = await exportService.PreflightAsync(new List<string> { "/test" }.ToArray());
		Assert.AreEqual(1, fileIndexResultsList.Count);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, fileIndexResultsList[0].Status);
	}
	
	[TestMethod]
	public async Task ExportService_Thumbnail_True()
	{
		var exportService = new ExportService(new FakeIQuery(), new AppSettings(), new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());
		var (zipHash, _) = await exportService.PreflightAsync(new List<string> { "/test" }.ToArray(), false, true);

		Assert.IsTrue( zipHash.StartsWith("TN"));
	}
	
	[TestMethod]
	public async Task ExportService_Thumbnail_False()
	{
		var exportService = new ExportService(new FakeIQuery(), new AppSettings(), new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());
		var (zipHash, _) = await exportService.PreflightAsync(new List<string> { "/test" }.ToArray(), false);

		Assert.IsTrue( zipHash.StartsWith("SR"));
	}

	[TestMethod]
	public async Task FilePathToFileNameAsync_NotfoundIsNull()
	{
		var exportService = new ExportService(new FakeIQuery(), new AppSettings(), new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());
		var fileName = await exportService.FilePathToFileNameAsync(new List<string>{"/test/not_found.jpg"}.ToArray(), true);
		Assert.AreEqual(null, fileName[0]);
	}
}
