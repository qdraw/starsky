using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starskycore.ViewModels;

namespace starskytest.ViewModels
{
	[TestClass]
	public class SearchViewModelTest
	{
		[TestMethod]
		public void SearchViewModelTest_TestIfTrash()
		{
			var test = new SearchViewModel {SearchQuery = "!delete!"};
			Assert.AreEqual(PageViewType.PageType.Trash.ToString(),test.PageType);
		}
		
		[TestMethod]
		public void SearchViewModelTest_Default()
		{
			var test = new SearchViewModel {};
			Assert.AreEqual(PageViewType.PageType.Search.ToString(),test.PageType);
		}
	}
}
