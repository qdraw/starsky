using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	[TestClass]
	public class ReplaceServiceTest
	{
		
		private ReplaceService _replace;
		private readonly Query _query;
		private readonly IMemoryCache _memoryCache;
		private FakeIStorage _iStorage;

		public ReplaceServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(ReplaceService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext,_memoryCache);

			_iStorage = new FakeIStorage(new List<string>{"/"}, new List<string>{"/test.jpg","/test2.jpg"});
			_replace = new ReplaceService(_query,new AppSettings(),_iStorage);

		}

		[TestMethod]
		public void ReplaceServiceTest_replaceString()
		{
	
			var item1 = _query.AddItem(new FileIndexItem
			{
				FileName = "test2.jpg",
				ParentDirectory = "/",
				Tags = "test1, !delete!, test"
			}); 
			
			var output = _replace.Replace("/test2.jpg",nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);

			_query.RemoveItem(item1);
		}

		[TestMethod]
		public void ReplaceServiceTest_replaceStringMultipleItems()
		{
			var item0 = _query.AddItem(new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/",
				Tags = "!delete!"
			});
			
			var item1 = _query.AddItem(new FileIndexItem
			{
				FileName = "test2.jpg",
				ParentDirectory = "/",
				Tags = "test1, !delete!, test"
			}); 
			var output = _replace.Replace("/test.jpg;/test2.jpg",nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);
			
			_query.RemoveItem(item0);
			_query.RemoveItem(item1);
		}
	}
}
