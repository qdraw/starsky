using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starskycore.ViewModels;

namespace starskytest.ViewModels
{
	[TestClass]
	public sealed class SyncViewModelTest
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
