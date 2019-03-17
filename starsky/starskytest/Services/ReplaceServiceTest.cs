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
		
		// todo: not found

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

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);
			Assert.AreEqual("test1, test",output[0].Tags);
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
			var output = _replace.Replace("/test2.jpg;/test.jpg",nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);

			Assert.AreEqual(string.Empty,output[1].Tags);

			_query.RemoveItem(item0);
			_query.RemoveItem(item1);
		}

		[TestMethod]
		public void ReplaceServiceTest_replaceStringWithNothingNull()
		{
			var item0 = _query.AddItem(new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/",
				Tags = "!delete!"
			});
			
			var output = _replace.Replace("/test2.jpg;/test.jpg",nameof(FileIndexItem.Tags),"!delete!",null,false);
			
			_query.RemoveItem(item0);

		}

		[TestMethod]
		public void ReplaceServiceTest_replaceSearchNull()
		{
			// When you search for nothing, there is nothing to replace 
			var output = _replace.Replace("/nothing.jpg", nameof(FileIndexItem.Tags), null, "test", false);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,output[0].Status);

		}


		[TestMethod]
		public void ReplaceServiceTest_replace_LowerCaseTagName()
		{
			var item1 = _query.AddItem(new FileIndexItem
			{
				FileName = "test2.jpg",
				ParentDirectory = "/",
				Tags = "test1, !delete!, test"
			}); 
			
			var output = _replace.Replace("/test2.jpg",nameof(FileIndexItem.Tags).ToLowerInvariant(),"!delete!",string.Empty,false);

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);
			Assert.AreEqual("test1, test",output[0].Tags);
			
			_query.RemoveItem(item1);
		}
	}
}
