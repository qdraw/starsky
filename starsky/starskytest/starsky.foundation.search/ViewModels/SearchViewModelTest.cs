using System;
using System.Collections.Generic;
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
			var test = new SearchViewModel { SearchQuery = TrashKeyword.TrashKeywordString };
			Assert.AreEqual(PageViewType.PageType.Trash.ToString(), test.PageType);
		}

		[TestMethod]
		public void SearchViewModelTest_Default()
		{
			var test = new SearchViewModel();
			Assert.AreEqual(PageViewType.PageType.Search.ToString(), test.PageType);
		}

		[TestMethod]
		public void SetAddSearchInStringType_Null()
		{
			var model = new SearchViewModel();

			model.GetType().GetProperty(nameof(model.SearchIn))?.SetValue(model, null, null);

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

			model.GetType().GetProperty(nameof(model.SearchForInternal))
				?.SetValue(model, null, null);

			model.SetAddSearchFor("");

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

			model.GetType().GetProperty(nameof(model.SearchForOptionsInternal))
				?.SetValue(model, null, null);

			model.SetAddSearchForOptions("test");

			Assert.AreNotEqual(null, model.SearchForOptions);
		}

		[TestMethod]
		public void SearchForOptions_SetAddSearchForOptions_DotComma()
		{
			var model = new SearchViewModel { SearchForOptionsInternal = null };

			model.GetType().GetProperty(nameof(model.SearchForOptionsInternal))
				?.SetValue(model, null, null);

			model.SetAddSearchForOptions(";");

			Assert.AreEqual(SearchViewModel.SearchForOptionType.Equal,
				model.SearchForOptions.LastOrDefault());
		}

		[TestMethod]
		public void SetAndOrOperator_amp_False()
		{
			var model = new SearchViewModel();
			model.SetAndOrOperator('&');

			Assert.AreNotEqual(false, model.SearchOperatorOptions.LastOrDefault());
		}

		[TestMethod]
		public void SearchOperatorContinue_IgnoreNegativeValue()
		{
			var model = new SearchViewModel { SearchOperatorOptionsInternal = new List<bool>() };
			var result = model.SearchOperatorContinue(-1, 1);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void SearchOperatorContinue_IgnoreOutOfRange()
		{
			var model = new SearchViewModel { SearchOperatorOptionsInternal = new List<bool>() };
			var result = model.SearchOperatorContinue(10, 1);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void SearchOperatorContinue_IgnoreOutOfRange2()
		{
			var model = new SearchViewModel { SearchOperatorOptionsInternal = new List<bool>() };
			var result = model.SearchOperatorContinue(0, 1);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void NarrowSearch_NoFileIndexItems()
		{
			var result = SearchViewModel.NarrowSearch(new SearchViewModel());
			Assert.AreEqual(0, result.SearchCount);
		}

		[TestMethod]
		public void SearchViewModel_ElapsedSeconds_Test()
		{
			var searchViewModel = new SearchViewModel { ElapsedSeconds = 0.0006 };
			Assert.AreEqual(true, searchViewModel.ElapsedSeconds <= 0.001);
		}

		[TestMethod]
		public void SearchViewModel_Offset_Test()
		{
			var searchViewModel = new SearchViewModel();
			Assert.AreEqual(0, Math.Floor(searchViewModel.Offset));
		}

		[TestMethod]
		public void PropertySearchTest()
		{
			var property = new FileIndexItem { Tags = "q" }.GetType()
				.GetProperty(nameof(FileIndexItem.Tags))!;

			// not a great test
			var search = SearchViewModel.PropertySearch(new SearchViewModel { SearchFor = { "q" } },
				property,
				"q", SearchViewModel.SearchForOptionType.Equal);

			Assert.AreEqual(0, search.CollectionsCount);
		}

		[TestMethod]
		public void PropertySearchStringType_DefaultCase_NullConditions1()
		{
			// Arrange
			var model = new SearchViewModel { FileIndexItems = null };

			var property = typeof(FileIndexItem).GetProperty("NotFound");
			Assert.IsNull(property);

			const string searchForQuery = "file";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result =
				SearchViewModel.PropertySearchStringType(model, property!, searchForQuery,
					searchType);

			// Assert
			Assert.IsNotNull(result);
			Assert.IsNull(result.FileIndexItems);
		}

		[TestMethod]
		public void PropertySearchStringType_DefaultCase_NullConditions2()
		{
			// Arrange
			var model = new SearchViewModel { FileIndexItems = null };

			var property = typeof(FileIndexItem).GetProperty(nameof(FileIndexItem.Tags));
			const string searchForQuery = "file";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result =
				SearchViewModel.PropertySearchStringType(model, property!, searchForQuery,
					searchType);

			// Assert
			Assert.IsNotNull(result);
			Assert.IsNull(result.FileIndexItems);
		}

		[TestMethod]
		public void PropertySearchStringType_DefaultCase_Found_Null()
		{
			// Arrange
			var model = new SearchViewModel
			{
				FileIndexItems = new List<FileIndexItem>
				{
					new FileIndexItem("test") { LocationCity = null }
				}
			};

			var property = typeof(FileIndexItem).GetProperty(nameof(FileIndexItem.LocationCity));
			const string searchForQuery = "file";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result =
				SearchViewModel.PropertySearchStringType(model, property!, searchForQuery,
					searchType);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.FileIndexItems?.Count);
		}

		[TestMethod]
		public void PropertySearchStringType_DefaultCase_Found_HappyFlow()
		{
			// Arrange
			var model = new SearchViewModel
			{
				FileIndexItems = new List<FileIndexItem>
				{
					new FileIndexItem("test") { LocationCity = "test" }
				}
			};

			var property = typeof(FileIndexItem).GetProperty(nameof(FileIndexItem.LocationCity));
			const string searchForQuery = "test";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result =
				SearchViewModel.PropertySearchStringType(model, property!, searchForQuery,
					searchType);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.FileIndexItems?.Count);
		}

		[TestMethod]
		public void PropertySearchBoolType_FiltersItemsByBoolProperty()
		{
			// Arrange
			var model = new SearchViewModel();
			model.FileIndexItems = new List<FileIndexItem>
			{
				new FileIndexItem { IsDirectory = true },
				new FileIndexItem { IsDirectory = false },
				new FileIndexItem { IsDirectory = true },
			};
			var property = typeof(FileIndexItem).GetProperty("IsDirectory");
			var boolIsValue = true;

			// Act
			var result = SearchViewModel.PropertySearchBoolType(model, property, boolIsValue);

			// Assert
			Assert.AreEqual(2, result.FileIndexItems?.Count);
			Assert.IsTrue(result.FileIndexItems?.Exists(item => item.IsDirectory == true));
		}

		[TestMethod]
		public void PropertySearchBoolType_WithNullModel_ReturnsNullModel()
		{
			// Arrange
			SearchViewModel? model = null;
			var property = typeof(FileIndexItem).GetProperty("IsDirectory");
			const bool boolIsValue = true;

			// Act
			var result = SearchViewModel.PropertySearchBoolType(model, property, boolIsValue);

			// Assert
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void PropertySearchBoolType_WithNullFileIndexItems_ReturnsNullFileIndexItems()
		{
			// Arrange
			var model = new SearchViewModel { FileIndexItems = null, };
			var property = typeof(FileIndexItem).GetProperty("IsDirectory");
			var boolIsValue = true;

			// Act
			var result = SearchViewModel.PropertySearchBoolType(model, property, boolIsValue);

			// Assert
			Assert.IsNull(result.FileIndexItems);
		}

		[TestMethod]
		public void PropertySearchBoolType_WithEmptyFileIndexItems_ReturnsEmptyFileIndexItems()
		{
			// Arrange
			var model = new SearchViewModel { FileIndexItems = new List<FileIndexItem>(), };
			var property = typeof(FileIndexItem).GetProperty("IsDirectory");
			var boolIsValue = true;

			// Act
			var result = SearchViewModel.PropertySearchBoolType(model, property, boolIsValue);

			// Assert
			Assert.IsNotNull(result.FileIndexItems);
			Assert.AreEqual(0, result.FileIndexItems.Count);
		}

		[TestMethod]
		public void PropertySearchBoolType_WithInvalidProperty_ReturnsOriginalModel()
		{
			// Arrange
			var model = new SearchViewModel();
			model.FileIndexItems = new List<FileIndexItem>
			{
				new FileIndexItem { IsDirectory = true },
			};
			var property = typeof(FileIndexItem).GetProperty("NonExistentProperty");
			var boolIsValue = true;

			// Act
			var result = SearchViewModel.PropertySearchBoolType(model, property, boolIsValue);

			// Assert
			Assert.AreEqual(model, result);
		}

		[TestMethod]
		public void PropertySearch_WithBoolPropertyAndValidBoolValue_ReturnsFilteredModel()
		{
			// Arrange
			var model = new SearchViewModel
			{
				FileIndexItems = new List<FileIndexItem>
				{
					new FileIndexItem { IsDirectory = true },
					new FileIndexItem { IsDirectory = false },
					new FileIndexItem { IsDirectory = true },
				}
			};
			var property = typeof(FileIndexItem).GetProperty("IsDirectory");
			const string searchForQuery = "true";
			const SearchViewModel.SearchForOptionType searchType =
				SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result =
				SearchViewModel.PropertySearch(model, property!, searchForQuery, searchType);

			// Assert
			Assert.AreEqual(2, result.FileIndexItems?.Count);
			Assert.IsTrue(result.FileIndexItems?.Exists(item => item.IsDirectory == true));
		}

		[TestMethod]
		public void PropertySearch_WithBoolPropertyAndInvalidBoolValue_ReturnsOriginalModel()
		{
			// Arrange
			var model = new SearchViewModel();
			model.FileIndexItems = new List<FileIndexItem>
			{
				new FileIndexItem { IsDirectory = true },
				new FileIndexItem { IsDirectory = false },
			};
			var property = typeof(FileIndexItem).GetProperty("IsDirectory");
			const string searchForQuery = "invalid_bool_value"; // An invalid boolean string
			const SearchViewModel.SearchForOptionType searchType =
				SearchViewModel.SearchForOptionType.Equal; // You can set this as needed

			// Act
			var result =
				SearchViewModel.PropertySearch(model, property!, searchForQuery, searchType);

			// Assert
			CollectionAssert.AreEqual(model.FileIndexItems, result.FileIndexItems);
		}
	}
}
