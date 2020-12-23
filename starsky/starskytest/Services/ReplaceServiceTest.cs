using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	[TestClass]
	public class ReplaceServiceTest
	{
		
		private MetaReplaceService _metaReplace;
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
			builder.UseInMemoryDatabase(nameof(MetaReplaceService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext,_memoryCache);

			_iStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg","/test2.jpg", "/readonly/test.jpg"});
			_metaReplace = new MetaReplaceService(_query,new AppSettings{ ReadOnlyFolders = new List<string>{"/readonly"}},
				new FakeSelectorStorage(_iStorage));

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
			
			var output = _metaReplace.Replace("/test2.jpg",
				nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);
			Assert.AreEqual("test1, test",output[0].Tags);
			_query.RemoveItem(item1);
		}

		[TestMethod]
		public void SearchAndReplace_Nothing()
		{
			var result = _metaReplace.SearchAndReplace(
				new List<FileIndexItem> {new FileIndexItem("/test.jpg"){Status = FileIndexItem.ExifStatus.Ok}},
				"tags", "test", string.Empty);

			Assert.AreEqual(string.Empty,result[0].Tags);
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
			var output = _metaReplace.Replace("/test2.jpg;/test.jpg",
				nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);
			
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
			
			var output = _metaReplace.Replace("/test2.jpg;/test.jpg",
				nameof(FileIndexItem.Tags),"!delete!",null,false);
			
			_query.RemoveItem(item0);

		}

		[TestMethod]
		public void ReplaceServiceTest_replaceSearchNull()
		{
			// When you search for nothing, there is nothing to replace 
			var output = _metaReplace.Replace("/nothing.jpg", nameof(FileIndexItem.Tags), 
				null, "test", false);
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
			
			var output = _metaReplace.Replace("/test2.jpg",
				nameof(FileIndexItem.Tags).ToLowerInvariant(),"!delete!",string.Empty,false);

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);
			Assert.AreEqual("test1, test",output[0].Tags);
			
			_query.RemoveItem(item1);
		}

		[TestMethod]
		public void ReplaceServiceTest_Readonly()
		{
			var item0 = _query.AddItem(new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/readonly",
				Tags = "!delete!"
			});
			
			var output = _metaReplace.Replace("/readonly/test.jpg",
				nameof(FileIndexItem.Tags),"!delete!",null,false);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, output.FirstOrDefault().Status);
			
			_query.RemoveItem(item0);
		}

		[TestMethod]
		public void SearchAndReplace_ReplaceDeletedTag_Default()
		{
			var items = new List<FileIndexItem>{new FileIndexItem{Tags = "test, !keyword!", Status = FileIndexItem.ExifStatus.Ok}};
			var result =  _metaReplace.SearchAndReplace(items, "Tags", "!keyword!", "");
			Assert.AreEqual("test",result.FirstOrDefault().Tags);
		}
		
		[TestMethod]
		public void SearchAndReplace_ReplaceDeletedTag_LowerCase()
		{
			var items = new List<FileIndexItem>{new FileIndexItem{Tags = "test, !keyword!", Status = FileIndexItem.ExifStatus.Ok}};
			var result =  _metaReplace.SearchAndReplace(items, "tags", "!keyword!", "");
			Assert.AreEqual("test",result.FirstOrDefault().Tags);
		}
		
		[TestMethod]
		public void SearchAndReplace_ReplaceDeletedTag_StatusDeleted()
		{
			var items = new List<FileIndexItem>{new FileIndexItem{Tags = "test, !delete!", Status = FileIndexItem.ExifStatus.Deleted}};
			var result =  _metaReplace.SearchAndReplace(items, "tags", "!delete!", "");
			Assert.AreEqual("test",result.FirstOrDefault().Tags);
		}
	}
}
