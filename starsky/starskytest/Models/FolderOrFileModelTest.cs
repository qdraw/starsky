using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Models;

namespace starskytest.Models
{
	[TestClass]
	public sealed class FolderOrFileModelTest
	{
		[TestMethod]
		public void FolderOrFileModelFolderOrFileTypeListTest()
		{
			var ToSearchType = FolderOrFileModel.FolderOrFileTypeList.Folder;
			var folderOrFileModel = new FolderOrFileModel 
			{
				IsFolderOrFile = ToSearchType
			};
			Assert.AreEqual(folderOrFileModel.IsFolderOrFile,ToSearchType);
		}

	}
}
