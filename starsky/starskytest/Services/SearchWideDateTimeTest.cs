using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;

namespace starskytest.Services
{
	[TestClass]
	public class SearchWideDateTimeTest
	{
		[TestMethod]
		public void SearchWideDateTimeTest_non_exist()
		{
			new SearchService().SearchDirect("-non0exist=test");
		}

		[TestMethod]
		public void SearchWideDateTimeTest_LastEdited_Equal()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions("=");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);
			
			var dateTime = new SearchViewModel().ParseDateTime("2015-01-01T01:01:01");
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited == dateTime);
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
		
		[TestMethod]
		public void SearchWideDateTimeTest_LastEdited_SmallerThen()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions("<");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);
			
			var dateTime = new SearchViewModel().ParseDateTime("2015-01-01T01:01:01");
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited <= dateTime);
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
		
		[TestMethod]
		public void SearchWideDateTimeTest_LastEdited_GreaterThen()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions(">");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);
			
			var dateTime = new SearchViewModel().ParseDateTime("2015-01-01T01:01:01");
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited >= dateTime);
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
		
		[TestMethod]
		public void SearchWideDateTimeTest_LastEdited_Between()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions(">");
			inputModel.SetAddSearchFor("2015-01-02T01:01:01");
			inputModel.SetAddSearchForOptions("<");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.LastEdited);
			
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited >= new DateTime() && p.LastEdited <= new DateTime());
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
		
		
		[TestMethod]
		public void SearchWideDateTimeTest_AddToDatabase_Equal()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions("=");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);
			
			var dateTime = new SearchViewModel().ParseDateTime("2015-01-01T01:01:01");
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited == dateTime);
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
		
		[TestMethod]
		public void SearchWideDateTimeTest_AddToDatabase_SmallerThen()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions("<");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);
			
			var dateTime = new SearchViewModel().ParseDateTime("2015-01-01T01:01:01");
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited <= dateTime);
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
		
		[TestMethod]
		public void SearchWideDateTimeTest_AddToDatabase_GreaterThen()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions(">");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);
			
			var dateTime = new SearchViewModel().ParseDateTime("2015-01-01T01:01:01");
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited >= dateTime);
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
		
		[TestMethod]
		public void SearchWideDateTimeTest_AddToDatabase_Between()
		{
			var inputModel = new SearchViewModel();
			inputModel.SetAddSearchFor("2015-01-01T01:01:01");
			inputModel.SetAddSearchForOptions(">");
			inputModel.SetAddSearchFor("2015-01-02T01:01:01");
			inputModel.SetAddSearchForOptions("<");

			var result = new SearchWideDateTime().WideSearchDateTimeGet(inputModel, 
				0,SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase);
			
			Expression<Func<FileIndexItem,bool>> expectedResult = (p => p.LastEdited >= new DateTime() && p.LastEdited <= new DateTime());
			
			Assert.AreEqual(expectedResult.Body.Type,result.Body.Type);
			Assert.AreEqual(expectedResult.Body.NodeType,result.Body.NodeType);
			Assert.AreEqual(expectedResult.Parameters.FirstOrDefault().Type,result.Parameters.FirstOrDefault().Type);
		}
	}
}
