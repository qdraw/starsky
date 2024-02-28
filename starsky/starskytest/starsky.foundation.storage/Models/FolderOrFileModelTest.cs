using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Models;

namespace starskytest.starsky.foundation.storage.Models
{
	[TestClass]
	public sealed class FolderOrFileModelTest
	{
		[TestMethod]
		public void FolderOrFileModelFolderOrFileTypeListTest()
		{
			const FolderOrFileModel.FolderOrFileTypeList searchType =
				FolderOrFileModel.FolderOrFileTypeList.Folder;
			var folderOrFileModel = new FolderOrFileModel { IsFolderOrFile = searchType };
			Assert.AreEqual(searchType, folderOrFileModel.IsFolderOrFile);
		}
	}
}
