using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryGetObjectsByFilePathAsyncTest
	{
		private readonly IQuery _query;

		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryGetObjectsByFilePathAsyncTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		public QueryGetObjectsByFilePathAsyncTest()
		{
			_query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetService<ApplicationDbContext>()) ;
		}

		
		[TestMethod]
		public async Task GetObjectsByFilePathAsync_SingleItem()
		{
			await _query.AddRangeAsync(new List<FileIndexItem>
			{
				new FileIndexItem("/single_item1.jpg"),
				new FileIndexItem("/single_item2.jpg")
			});
			
			var result = await _query.GetObjectsByFilePathAsync(
				new List<string> {"/single_item1.jpg"});

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("/single_item1.jpg",result[0].FilePath);

			await _query.RemoveItemAsync(result[0]);
			
			await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/single_item2.jpg"));
		}

		[TestMethod] 
		public async Task GetObjectsByFilePathAsync_Single_ButDuplicate_Item()
		{
			await _query.AddRangeAsync(new List<FileIndexItem>
			{
				new FileIndexItem("/single_duplicate.jpg"),
				new FileIndexItem("/single_duplicate.jpg")
			});
			
			var result = await _query.GetObjectsByFilePathAsync(
				new List<string> {"/single_duplicate.jpg"});

			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("/single_duplicate.jpg",result[0].FilePath);
			Assert.AreEqual("/single_duplicate.jpg",result[1].FilePath);

			await _query.RemoveItemAsync(result[0]);
			await _query.RemoveItemAsync(result[1]);
		}
		
		[TestMethod]
		public async Task GetObjectsByFilePathAsync_MultipleItems()
		{
			await _query.AddRangeAsync(new List<FileIndexItem>
			{
				new FileIndexItem("/multiple_item_0.jpg"),
				new FileIndexItem("/multiple_item_1.jpg")
			});
			
			var result = await _query.GetObjectsByFilePathAsync(
				new List<string> {"/multiple_item_0.jpg", "/multiple_item_1.jpg"});

			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("/multiple_item_1.jpg",result[0].FilePath);
			Assert.AreEqual("/multiple_item_0.jpg",result[1].FilePath);

			await _query.RemoveItemAsync(result[0]);
			await _query.RemoveItemAsync(result[1]);
		}
	}
}
