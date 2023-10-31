using System;
using System.Collections.Generic;
using System.Linq;
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
			Assert.AreEqual(0,Math.Floor(searchViewModel.Offset));
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
		
		[TestMethod]
		public void PropertySearchStringType_DefaultCase_NullConditions1()
		{
			// Arrange
			var model = new SearchViewModel
			{
				FileIndexItems = null
			};

			var property = typeof(FileIndexItem).GetProperty("NotFound");
			Assert.IsNull(property);
			
			const string searchForQuery = "file";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result = SearchViewModel.PropertySearchStringType(model, property, searchForQuery, searchType);

			// Assert
			Assert.IsNotNull(result);
			Assert.IsNull(result.FileIndexItems);
		}
		
		[TestMethod]
		public void PropertySearchStringType_DefaultCase_NullConditions2()
		{
			// Arrange
			var model = new SearchViewModel
			{
				FileIndexItems = null
			};

			var property = typeof(FileIndexItem).GetProperty(nameof(FileIndexItem.Tags));
			const string searchForQuery = "file";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result = SearchViewModel.PropertySearchStringType(model, property, searchForQuery, searchType);

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
				FileIndexItems = new List<FileIndexItem>{new FileIndexItem("test"){LocationCity = null}}
			};

			var property = typeof(FileIndexItem).GetProperty(nameof(FileIndexItem.LocationCity));
			const string searchForQuery = "file";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result = SearchViewModel.PropertySearchStringType(model, property, searchForQuery, searchType);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0,result.FileIndexItems?.Count);
		}
		
		[TestMethod]
		public void PropertySearchStringType_DefaultCase_Found_HappyFlow()
		{
			// Arrange
			var model = new SearchViewModel
			{
				FileIndexItems = new List<FileIndexItem>{new FileIndexItem("test"){LocationCity = "test"}}
			};

			var property = typeof(FileIndexItem).GetProperty(nameof(FileIndexItem.LocationCity));
			const string searchForQuery = "test";
			var searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result = SearchViewModel.PropertySearchStringType(model, property, searchForQuery, searchType);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1,result.FileIndexItems?.Count);
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
			Assert.IsTrue(result.FileIndexItems?.All(item => item.IsDirectory == true));
		}

		[TestMethod]
		public void PropertySearchBoolType_WithNullModel_ReturnsNullModel()
		{
			// Arrange
			SearchViewModel model = null;
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
			var model = new SearchViewModel
			{
				FileIndexItems = null,
			};
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
			var model = new SearchViewModel
			{
				FileIndexItems = new List<FileIndexItem>(),
			};
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
			var model = new SearchViewModel { FileIndexItems = new List<FileIndexItem>
				{
					new FileIndexItem { IsDirectory = true },
					new FileIndexItem { IsDirectory = false },
					new FileIndexItem { IsDirectory = true },
				}
			};
			var property = typeof(FileIndexItem).GetProperty("IsDirectory");
			const string searchForQuery = "true"; 
			const SearchViewModel.SearchForOptionType searchType = SearchViewModel.SearchForOptionType.Equal;

			// Act
			var result = SearchViewModel.PropertySearch(model, property, searchForQuery, searchType);

			// Assert
			Assert.AreEqual(2, result.FileIndexItems?.Count);
			Assert.IsTrue(result.FileIndexItems?.All(item => item.IsDirectory == true));
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
			var searchForQuery = "invalid_bool_value"; // An invalid boolean string
			var searchType = SearchViewModel.SearchForOptionType.Equal; // You can set this as needed

			// Act
			var result = SearchViewModel.PropertySearch(model, property, searchForQuery, searchType);

			// Assert
			CollectionAssert.AreEqual(model.FileIndexItems, result.FileIndexItems);
		}
	}
}
