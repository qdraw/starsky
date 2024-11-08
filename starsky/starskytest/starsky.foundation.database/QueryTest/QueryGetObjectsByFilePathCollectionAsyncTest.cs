using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryGetObjectsByFilePathCollectionAsyncTest
{
	private readonly Query _query;

	public QueryGetObjectsByFilePathCollectionAsyncTest()
	{
		_query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(),
			new AppSettings(), CreateNewScope(), new FakeIWebLogger(), new FakeMemoryCache());
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryGetObjectsByFilePathAsyncTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_SingleItem_2()
	{
		await Add();

		var result = await _query.GetObjectsByFilePathCollectionAsync("/single_item1_async.jpg");
		if ( result.Count != 1 )
		{
			await Add();
			result = await _query.GetObjectsByFilePathCollectionAsync("/single_item1_async.jpg");
		}

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/single_item1_async.jpg", result[0].FilePath);

		await _query.RemoveItemAsync(result[0]);

		var item =
			await _query.GetObjectByFilePathAsync(
				"/single_item2_async.jpg");
		Assert.IsNotNull(item);
		await _query.RemoveItemAsync(item);
		return;

		async Task Add()
		{
			await _query.AddRangeAsync(new List<FileIndexItem>
			{
				new("/single_item1_async.jpg"), new("/single_item2_async.jpg")
			});
		}
	}

	[TestMethod]
	[DataRow("/single_item1_collection.jpg")]
	[DataRow("/")]
	public async Task GetObjectsByFilePathCollectionAsync_SingleItem(string filePathToSearchFor)
	{
		var addedItems = new List<FileIndexItem>
		{
			new("/"), new("/single_item1_collection.jpg"), new("/single_item2_collection.jpg")
		};
		await _query.AddRangeAsync(addedItems);

		var result = await _query.GetObjectsByFilePathCollectionQueryAsync(
			new List<string> { filePathToSearchFor });

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(filePathToSearchFor, result[0].FilePath);

		await _query.RemoveItemAsync(addedItems);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathCollectionAsync_SingleItem_LookAlikeStartsWithName()
	{
		await _query.AddRangeAsync(new List<FileIndexItem> { new("/3.jpg"), new("/3020.jpg") });

		var result = await _query.GetObjectsByFilePathCollectionQueryAsync(
			new List<string> { "/3.jpg" });

		Assert.AreEqual(1, result.Count(p => p.FileName?.StartsWith('3') == true));
		var threeJpg =
			result.Where(p => p.FileName?.StartsWith('3') == true)
				.ToList()[0];
		Assert.AreEqual("/3.jpg", threeJpg.FilePath);

		var three20 = await _query.GetObjectByFilePathAsync("/3020.jpg");
		await _query.RemoveItemAsync(threeJpg);

		Assert.AreEqual("/3020.jpg", three20?.FilePath);
		await _query.RemoveItemAsync(three20!);
	}


	[TestMethod]
	public async Task GetObjectsByFilePathCollectionAsync_SingleItem_NoExtension()
	{
		await _query.AddRangeAsync(new List<FileIndexItem> { new("/2"), new("/2020") });

		var result = await _query.GetObjectsByFilePathCollectionQueryAsync(
			new List<string> { "/2" });

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/2", result[0].FilePath);

		await _query.RemoveItemAsync(result[0]);

		await _query.RemoveItemAsync(( await _query.GetObjectByFilePathAsync("/2020") )!);
	}


	[TestMethod]
	public async Task GetObjectsByFilePathCollectionAsync_Single_ButDuplicate_Item()
	{
		const string subPath = "/single_duplicate_2.jpg";

		async Task Add()
		{
			await _query.AddRangeAsync(new List<FileIndexItem> { new(subPath), new(subPath) });
		}

		await Add();

		var result = await _query.GetObjectsByFilePathCollectionQueryAsync(
			new List<string> { subPath });
		if ( result.Count != 2 )
		{
			await Add();
			result = await _query.GetObjectsByFilePathCollectionQueryAsync(
				new List<string> { subPath });
		}

		Assert.AreEqual(2, result.Count);
		Assert.AreEqual(subPath, result[0].FilePath);
		Assert.AreEqual(subPath, result[1].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathCollectionAsync_MultipleItems()
	{
		async Task AddExampleRange()
		{
			await _query.AddRangeAsync(new List<FileIndexItem>
			{
				new("/test__multiple_item"), // <= should never match this one
				new("/test__multiple_item_0.jpg"),
				new("/test__multiple_item_1.jpg"),
				new("/test__multiple_item_2.jpg"),
				new("/test__multiple_item_3.jpg")
			});
		}

		async Task<List<FileIndexItem>> ExampleQuery()
		{
			return await _query.GetObjectsByFilePathCollectionQueryAsync(
				new List<string>
				{
					"/test__multiple_item_0.jpg",
					"/test__multiple_item_1.jpg",
					"/test__multiple_item_2.jpg",
					"/test__multiple_item_3.jpg"
				});
		}


		var beforeResult = await ExampleQuery();
		foreach ( var item in beforeResult )
		{
			await _query.RemoveItemAsync(item);
		}

		await AddExampleRange();

		var result = await ExampleQuery();
		if ( result.Count == 0 )
		{
			await Task.Delay(100);
			await AddExampleRange();
			result = await ExampleQuery();
		}

		Assert.AreEqual(4, result.Count);

		var orderedResults = result.OrderBy(p => p.FileName).ToList();
		Assert.AreEqual("/test__multiple_item_0.jpg", orderedResults[0].FilePath);
		Assert.AreEqual("/test__multiple_item_1.jpg", orderedResults[1].FilePath);
		Assert.AreEqual("/test__multiple_item_2.jpg", orderedResults[2].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
		await _query.RemoveItemAsync(result[2]);
		await _query.RemoveItemAsync(result[3]);
		var multipleItem = await _query.GetObjectByFilePathAsync("/test__multiple_item");
		if ( multipleItem != null )
		{
			await _query.RemoveItemAsync(multipleItem);
		}
	}

	[TestMethod]
	public async Task GetObjectsByFilePathCollectionAsync_TwoItems()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/two_item_0.jpg"), new("/two_item_1.jpg")
		});

		var result = await _query.GetObjectsByFilePathCollectionQueryAsync(
			new List<string> { "/two_item_0.jpg", "/two_item_1.jpg" });

		Assert.AreEqual(2, result.Count);

		var orderedResults = result.OrderBy(p => p.FileName).ToList();
		Assert.AreEqual("/two_item_0.jpg", orderedResults[0].FilePath);
		Assert.AreEqual("/two_item_1.jpg", orderedResults[1].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_Collection_SingleItem_Disposed()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/disposed2/single_item_disposed_1.jpg")
		});

		// get context
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		var result = await new Query(dbContextDisposed,
				new AppSettings(), serviceScopeFactory, new FakeIWebLogger(), new FakeMemoryCache())
			.GetObjectsByFilePathCollectionQueryAsync(new List<string>
			{
				"/disposed2/single_item_disposed_1.jpg"
			});

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/disposed2/single_item_disposed_1.jpg", result[0].FilePath);
	}
}
