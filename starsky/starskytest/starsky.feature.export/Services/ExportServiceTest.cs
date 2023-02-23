using System;
using System.Collections.Generic;
using System.Linq;
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
	public void Export_Folder()
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
		
		var (_,fileIndexResultsList) = exportService.Preflight(new List<string> { "/test" }.ToArray());
		
		Assert.AreEqual(1, fileIndexResultsList.Count);
	}
	
	[TestMethod]
	public void Export_Folder_StackCollection_True()
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
		
		var (_,fileIndexResultsList) = exportService.Preflight(new List<string> { "/test/test.jpg" }.ToArray());
		
		Assert.AreEqual(3, fileIndexResultsList.Count);
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.FilePath == "/test/test.jpg"));
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.FilePath == "/test/test.dng"));
	}
	
		
	[TestMethod]
	public void Export_Folder_StackCollection_False()
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
		
		var (_,fileIndexResultsList) = exportService.Preflight(new List<string> { "/test/test.jpg" }.ToArray(), false);
		
		Assert.AreEqual(1, fileIndexResultsList.Count);
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.FilePath == "/test/test.jpg"));
		Assert.AreEqual(0, fileIndexResultsList.Count(p => p.FilePath == "/test/test.dng"));
	}

	[TestMethod]
	public void Export_Ignore_Deleted_FolderFile()
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
		
		var (_,fileIndexResultsList) = exportService.Preflight(new List<string> { "/test" }.ToArray());
		
		Assert.AreEqual(0, fileIndexResultsList.Count);
	}
	
	
	[TestMethod]
	public void Export_Ignore_Deleted_Folder()
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
		
		var (_,fileIndexResultsList) = exportService.Preflight(new List<string> { "/test/test.jpg" }.ToArray());
		
		Assert.AreEqual(0, fileIndexResultsList.Count(p => p.Status == FileIndexItem.ExifStatus.Ok));
		Assert.AreEqual(1, fileIndexResultsList.Count(p => p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing));
	}
}
