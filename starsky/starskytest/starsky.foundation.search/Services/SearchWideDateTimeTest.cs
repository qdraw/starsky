using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.search.Services;
using starsky.feature.search.ViewModels;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.search.Services;

[TestClass]
public sealed class SearchWideDateTimeTest
{
	[TestMethod]
	public void SearchWideDateTimeTest_LastEdited_Equal()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions("=");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);

		var dateTime = SearchViewModel.ParseDateTime("2015-01-01T01:01:01");
		Expression<Func<FileIndexItem, bool>> expectedResult = p => p.LastEdited == dateTime;

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}

	[TestMethod]
	public void SearchWideDateTimeTest_LastEdited_SmallerThen()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions("<");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);

		var dateTime = SearchViewModel.ParseDateTime("2015-01-01T01:01:01");
		Expression<Func<FileIndexItem, bool>> expectedResult = p => p.LastEdited <= dateTime;

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}

	[TestMethod]
	public void SearchWideDateTimeTest_LastEdited_GreaterThen()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions(">");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);

		var dateTime = SearchViewModel.ParseDateTime("2015-01-01T01:01:01");
		Expression<Func<FileIndexItem, bool>> expectedResult = p => p.LastEdited >= dateTime;

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}

	[TestMethod]
	public void SearchWideDateTimeTest_LastEdited_Between()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions(">");
		inputModel.SetAddSearchFor("2015-01-02T01:01:01");
		inputModel.SetAddSearchForOptions("<");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);

		Expression<Func<FileIndexItem, bool>> expectedResult = p =>
			p.LastEdited >= new DateTime() && p.LastEdited <= new DateTime();

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}


	[TestMethod]
	public void SearchWideDateTimeTest_AddToDatabase_Equal()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions("=");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);

		var dateTime = SearchViewModel.ParseDateTime("2015-01-01T01:01:01");
		Expression<Func<FileIndexItem, bool>> expectedResult = p => p.LastEdited == dateTime;

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}

	[TestMethod]
	public void SearchWideDateTimeTest_AddToDatabase_SmallerThen()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions("<");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);

		var dateTime = SearchViewModel.ParseDateTime("2015-01-01T01:01:01");
		Expression<Func<FileIndexItem, bool>> expectedResult = p => p.LastEdited <= dateTime;

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}

	[TestMethod]
	public void SearchWideDateTimeTest_AddToDatabase_GreaterThen()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions(">");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);

		var dateTime = SearchViewModel.ParseDateTime("2015-01-01T01:01:01");
		Expression<Func<FileIndexItem, bool>> expectedResult = p => p.LastEdited >= dateTime;

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}

	[TestMethod]
	public void SearchWideDateTimeTest_AddToDatabase_Between()
	{
		var inputModel = new SearchViewModel();
		inputModel.SetAddSearchFor("2015-01-01T01:01:01");
		inputModel.SetAddSearchForOptions(">");
		inputModel.SetAddSearchFor("2015-01-02T01:01:01");
		inputModel.SetAddSearchForOptions("<");

		var result = SearchWideDateTime.WideSearchDateTimeGet(inputModel,
			0, SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);

		Expression<Func<FileIndexItem, bool>> expectedResult = p =>
			p.LastEdited >= new DateTime() && p.LastEdited <= new DateTime();

		Assert.AreEqual(expectedResult.Body.Type, result.Body.Type);
		Assert.AreEqual(expectedResult.Body.NodeType, result.Body.NodeType);
		Assert.AreEqual(expectedResult.Parameters.FirstOrDefault()?.Type,
			result.Parameters.FirstOrDefault()?.Type);
	}

	[TestMethod]
	public void WideSearchDateTimeGet_ArgumentException_BothGreaterSmaller()
	{
		// Arrange
		var model = new SearchViewModel();
		model.SetAddSearchFor("2023-01-01");
		model.SetAddSearchFor("2023-01-02");
		model.SetAddSearchForOptions(">");
		model.SetAddSearchForOptions("<");

		// Write enum with reflection

		// Act
		Assert.ThrowsException<ArgumentException>(() =>
		{
			SearchWideDateTime.WideSearchDateTimeGet(model, 0, InvalidEnum());
		});
	}
	
	[DataTestMethod] // [Theory]
	[DataRow("<")]
	[DataRow(">")]
	[DataRow("=")]
	public void WideSearchDateTimeGet_ArgumentNullException_LessGreaterThanInvalidEnum(string value)
	{
		// Arrange
		var model = new SearchViewModel();
		model.SetAddSearchFor("2023-01-02 12:00:00"); // specific date instead of day only
		model.SetAddSearchForOptions(value);
		
		// Act
		Assert.ThrowsException<ArgumentNullException>(() =>
		{
			SearchWideDateTime.WideSearchDateTimeGet(model, 0, InvalidEnum());
		});
	}

	/// <summary>
	/// Override the enum with reflection
	/// Set invalid enum value to trigger an exception
	/// </summary>
	/// <returns></returns>
	private static SearchWideDateTime.WideSearchDateTimeGetType InvalidEnum()
	{
		var myClass = new SetSearchWideDateTimeOverrideObject();
		var propertyObject = myClass.GetType().GetProperty("Type");

		// Set an invalid value that should trigger an exception
		propertyObject?.SetValue(myClass, 44, null);

		return myClass.Type;
	}

	private class SetSearchWideDateTimeOverrideObject
	{
		public SearchWideDateTime.WideSearchDateTimeGetType Type { get; set; }
	}
}
