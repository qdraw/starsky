using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.project.web.ViewModels;

namespace starskytest.ViewModels
{
	[TestClass]
	public sealed class ArchiveViewModelTest
	{
		[TestMethod]
		public void ArchiveViewModelPageTypeTest()
		{
			var viewModel = new ArchiveViewModel();
			Assert.AreEqual(PageViewType.PageType.Archive.ToString(), viewModel.PageType);
		}

		[TestMethod]
		public void ArchiveViewModel_ColorClass()
		{
			var viewModel = new ArchiveViewModel
			{
				ColorClassActiveList =
					new List<ColorClassParser.Color> { ColorClassParser.Color.None },
				ColorClassUsage =
					new List<ColorClassParser.Color> { ColorClassParser.Color.None }
			};

			Assert.AreEqual(ColorClassParser.Color.None,
				viewModel.ColorClassActiveList.FirstOrDefault());
			Assert.AreEqual(ColorClassParser.Color.None,
				viewModel.ColorClassUsage.FirstOrDefault());
		}

		[TestMethod]
		public void ArchiveViewModel_ExampleData()
		{
			var archiveViewModel = new ArchiveViewModel
			{
				FileIndexItems = new List<FileIndexItem>(),
				Breadcrumb = new List<string> { "/" },
				RelativeObjects = new RelativeObjects { NextFilePath = "/" },
				SearchQuery = "test",
				SubPath = "/",
				IsReadOnly = false,
				CollectionsCount = 0,
				Collections = true,
			};

			Assert.AreEqual("/", archiveViewModel.Breadcrumb.FirstOrDefault());
			Assert.AreEqual("/", archiveViewModel.RelativeObjects.NextFilePath);
			Assert.AreEqual("test", archiveViewModel.SearchQuery);
			Assert.AreEqual("/", archiveViewModel.SubPath);
			Assert.IsFalse(archiveViewModel.IsReadOnly);
			Assert.AreEqual(0, archiveViewModel.CollectionsCount);
			Assert.IsTrue(archiveViewModel.Collections);
		}
	}
}
