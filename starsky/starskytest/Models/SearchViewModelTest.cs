using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.search.ViewModels;
using starsky.foundation.database.Models;

namespace starskytest.Models
{
	[TestClass]
	public sealed class SearchViewModelTest
	{
		[TestMethod]
		public void SearchViewModel_ElapsedSeconds_Test()
		{
			var searchViewModel = new SearchViewModel{ElapsedSeconds = 0.0006};
			Assert.AreEqual(true, searchViewModel.ElapsedSeconds <= 0.001);
		}

		[TestMethod]
		public void SearchViewModel_Offset_Test()
		{
			var searchViewModel = new SearchViewModel();
			Assert.AreEqual(0,searchViewModel.Offset );
		}

		[TestMethod]
		public void PropertySearchTest()
		{
			var property = new FileIndexItem{Tags = "q"}.GetType().GetProperty(nameof(FileIndexItem.Tags))!;

			// not a great test
			var search = SearchViewModel.PropertySearch(new SearchViewModel{SearchFor = { "q" }}, property, 
				"q", SearchViewModel.SearchForOptionType.Equal);
			
			Assert.AreEqual(0, search.CollectionsCount);
		}
	}
}
