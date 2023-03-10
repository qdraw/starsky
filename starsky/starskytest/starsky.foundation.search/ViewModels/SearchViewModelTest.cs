using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.search.ViewModels;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.search.ViewModels
{
	[TestClass]
	public sealed class SearchViewModelTest
	{
		[TestMethod]
		public void SearchViewModelTest_TestIfTrash()
		{
			var test = new SearchViewModel {SearchQuery = TrashKeyword.TrashKeywordString};
			Assert.AreEqual(PageViewType.PageType.Trash.ToString(),test.PageType);
		}
		
		[TestMethod]
		public void SearchViewModelTest_Default()
		{
			var test = new SearchViewModel();
			Assert.AreEqual(PageViewType.PageType.Search.ToString(),test.PageType);
		}

		[TestMethod]
		public void SetAddSearchInStringType_Null()
		{
			var model = new SearchViewModel();
			
			model.GetType().GetProperty(nameof(model.SearchIn))?.SetValue(model, null,null);
			
			model.SetAddSearchInStringType(null!);
			
			Assert.AreNotEqual(null, model.SearchIn);
		}

		[TestMethod]
		public void SetAddSearch_searchForType_Null()
		{
			var model = new SearchViewModel { SearchForInternal = null };

			Assert.AreNotEqual(null, model.SearchFor);
		}
				
		[TestMethod]
		public void SetAddSearch_searchForType_SetAddSearchFor_Null()
		{
			var model = new SearchViewModel { SearchForInternal = null };

			model.GetType().GetProperty(nameof(model.SearchForInternal))?.SetValue(model, null,null);

			model.SetAddSearchInStringType(null!);
			
			Assert.AreNotEqual(null, model.SearchFor);
		}

		[TestMethod]
		public void SearchForOptions_Null()
		{
			var model = new SearchViewModel { SearchForOptionsInternal = null };

			Assert.AreNotEqual(null, model.SearchForOptions);
		}
				
		[TestMethod]
		public void SearchForOptions_SetAddSearchForOptions_Null()
		{
			var model = new SearchViewModel { SearchForOptionsInternal = null };

			model.GetType().GetProperty(nameof(model.SearchForOptionsInternal))?.SetValue(model, null,null);

			model.SetAddSearchForOptions("test");
			
			Assert.AreNotEqual(null, model.SearchForOptions);
		}
		
		[TestMethod]
		public void SearchForOptions_SetAddSearchForOptions_DotComma()
		{
			var model = new SearchViewModel { SearchForOptionsInternal = null };

			model.GetType().GetProperty(nameof(model.SearchForOptionsInternal))?.SetValue(model, null,null);

			model.SetAddSearchForOptions(";");
			
			Assert.AreEqual(SearchViewModel.SearchForOptionType.Equal, model.SearchForOptions.LastOrDefault());
		}
	}
}
