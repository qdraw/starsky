using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.ViewModels;
using starskycore.Models;

namespace starskytests.ViewModels
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