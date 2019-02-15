using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models;
using starskycore.ViewModels;

namespace starskytest.ViewModels
{
	[TestClass]
	public class SyncViewModelTest
	{
		[TestMethod]
		public void SyncViewModelSyncViewModelTest()
		{
			var syncViewModel = new SyncViewModel
			{
				FilePath = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}; 
            
			Assert.AreEqual("/test",syncViewModel.FilePath);  
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,syncViewModel.Status);  
		}
	}
}