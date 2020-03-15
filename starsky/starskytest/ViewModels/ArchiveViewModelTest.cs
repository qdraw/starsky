using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.query.Models;
using starskycore.Models;
using starskycore.ViewModels;

namespace starskytest.ViewModels
{
    [TestClass]
    public class ArchiveViewModelTest
    {
        [TestMethod]
        public void ArchiveViewModelPageTypeTest()
        {
            var viewModel = new ArchiveViewModel(); 
            Assert.AreEqual( PageViewType.PageType.Archive.ToString(),viewModel.PageType);            
        }
        
        [TestMethod]
        public void ArchiveViewModel_ColorClass()
        {
	        var viewModel = new ArchiveViewModel
	        {
		        ColorClassActiveList = new List<FileIndexItem.Color>{FileIndexItem.Color.None},
		        ColorClassUsage = new List<FileIndexItem.Color>{FileIndexItem.Color.None}
	        };
            
	        Assert.AreEqual(FileIndexItem.Color.None, viewModel.ColorClassActiveList.FirstOrDefault());            
	        Assert.AreEqual(FileIndexItem.Color.None, viewModel.ColorClassUsage.FirstOrDefault());            
        }
        
        [TestMethod]
        public void ArchiveViewModel_ExampleData()
        {
	        var archiveViewModel = new ArchiveViewModel
	        {
		        FileIndexItems = new List<FileIndexItem>(),
		        Breadcrumb = new List<string>{"/"},
		        RelativeObjects = new RelativeObjects
		        {
			        NextFilePath = "/"
		        },
		        SearchQuery = "test",
		        SubPath = "/",
		        IsReadOnly = false,
		        CollectionsCount= 0,
	        };
	        
	        Assert.AreEqual("/", archiveViewModel.Breadcrumb.FirstOrDefault());      
	        Assert.AreEqual("/", archiveViewModel.RelativeObjects.NextFilePath);      
	        Assert.AreEqual("test", archiveViewModel.SearchQuery);      
	        Assert.AreEqual("/", archiveViewModel.SubPath);      
	        Assert.AreEqual(false, archiveViewModel.IsReadOnly);      
	        Assert.AreEqual(0, archiveViewModel.CollectionsCount);      
        }
    }
}
