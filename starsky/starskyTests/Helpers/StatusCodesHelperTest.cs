using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Models;
using starsky.ViewModels;

namespace starskytests.Helpers
{
	[TestClass]
	public class StatusCodesHelperTest
	{
		[TestMethod]
		public void StatusCodesHelperTest_NotFoundNotInIndex()
		{
			var appSettings = new AppSettings();
			var status = new StatusCodesHelper(appSettings).FileCollectionsCheck(null);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_DirReadOnly()
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
		public void StatusCodesHelperTest_NotFoundIsDir()
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
		
	}
}
