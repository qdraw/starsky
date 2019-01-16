using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Models;
using starsky.ViewModels;
using starskycore.Helpers;
using starskycore.Models;

namespace starskytests.Helpers
{
	[TestClass]
	public class StatusCodesHelperTest
	{
		[TestMethod]
		public void StatusCodesHelperTest_FileCollectionsCheck_NotFoundNotInIndex()
		{
			var appSettings = new AppSettings();
			var status = new StatusCodesHelper(appSettings).FileCollectionsCheck(null);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_FileCollectionsCheck_DirReadOnly()
		{
			var appSettings = new AppSettings{ReadOnlyFolders = new List<string>{"/"}};
			var detailView = new DetailView
			{
				IsDirectory = true,
				SubPath = "/"
			};
			var status = new StatusCodesHelper(appSettings).FileCollectionsCheck(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.DirReadOnly,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_FileCollectionsCheck_NotFoundIsDir()
		{
			var appSettings = new AppSettings();
			var detailView = new DetailView
			{
				IsDirectory = true,
				SubPath = "/"
			};
			var status = new StatusCodesHelper(appSettings).FileCollectionsCheck(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundIsDir,status);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_NotFoundIsDir()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.NotFoundIsDir;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper(appSettings).ReturnExifStatusError(statusModel, statusResults,
					fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_DirReadOnly()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.DirReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper(appSettings).ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_NotFoundNotInIndex()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.NotFoundNotInIndex;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper(appSettings).ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_NotFoundSourceMissing()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.NotFoundSourceMissing;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper(appSettings).ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
	
		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_ReadOnly()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.ReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper(appSettings).ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
		
	}
}
