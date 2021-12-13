using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		
		private readonly MetaReplaceService _metaReplace;
		private readonly Query _query;
		private readonly FakeIStorage _iStorage;

		public ReplaceServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(MetaReplaceService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext, new AppSettings(), null, 
				new FakeIWebLogger(),memoryCache);
			
			_iStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg","/test2.jpg", "/readonly/test.jpg", "/test.dng"});
			_metaReplace = new MetaReplaceService(_query,new AppSettings{ ReadOnlyFolders = new List<string>{"/readonly"}},
				new FakeSelectorStorage(_iStorage), new FakeIWebLogger());

		}

		[TestMethod]
		public async Task ReplaceServiceTest_NotFound()
		{
			var output = await _metaReplace.Replace("/not-found.jpg",
				nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,output[0].Status);
		}
		
		[TestMethod]
		public async Task ReplaceServiceTest_NotFoundOnDiskButFoundInDatabase()
		{
			var item1 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "only-found-in-db.jpg",
				ParentDirectory = "/",
				Tags = "test1, test"
			}); 
			
			var output = await _metaReplace.Replace("/only-found-in-db.jpg",
				nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,output[0].Status);
			
			await _query.RemoveItemAsync(item1);
		}
		
		[TestMethod]
		public async Task ReplaceServiceTest_ToDeleteStatus()
		{
			var item1 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test2.jpg",
				ParentDirectory = "/",
				Tags = "test1, test"
			}); 
			
			var output = await _metaReplace.Replace("/test2.jpg",
				nameof(FileIndexItem.Tags),"test1","!delete!",false);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted,output[0].Status);
			Assert.AreEqual("!delete!, test",output[0].Tags);

			await _query.RemoveItemAsync(item1);
		}

		[TestMethod]
		public async Task ReplaceServiceTest_replaceString()
		{
			var item1 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test2.jpg",
				ParentDirectory = "/",
				Tags = "test1, !delete!, test"
			}); 
			
			var output = await _metaReplace.Replace("/test2.jpg",
				nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);
			Assert.AreEqual("test1, test",output[0].Tags);
			await _query.RemoveItemAsync(item1);
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
		public async Task ReplaceServiceTest_replaceStringMultipleItems()
		{
			var item0 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/",
				Tags = "!delete!"
			});
			
			var item1 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test2.jpg",
				ParentDirectory = "/",
				Tags = "test1, !delete!, test"
			}); 
			
			var output = await _metaReplace.Replace("/test2.jpg;/test.jpg",
				nameof(FileIndexItem.Tags),"!delete!",string.Empty,false);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);

			Assert.AreEqual(string.Empty,output.FirstOrDefault(p => p.FilePath == "/test.jpg").Tags);
			Assert.AreEqual("test1, test",output.FirstOrDefault(p => p.FilePath == "/test2.jpg").Tags);

			await _query.RemoveItemAsync(item0);
			await _query.RemoveItemAsync(item1);
		}
		
		
		[TestMethod]
		public async Task ReplaceServiceTest_replaceStringMultipleItemsCollections()
		{
			var item0 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/",
				Tags = "!delete!"
			});
			
			var item1 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test.dng",
				ParentDirectory = "/",
				Tags = "!delete!"
			}); 
			
			var output = await _metaReplace.Replace("/test.jpg",
				nameof(FileIndexItem.Tags),"!delete!",string.Empty,true);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[1].Status);

			Assert.AreEqual(string.Empty,output.FirstOrDefault(p => p.FilePath == "/test.jpg").Tags);
			Assert.AreEqual(string.Empty,output.FirstOrDefault(p => p.FilePath == "/test.dng").Tags);

			await _query.RemoveItemAsync(item0);
			await _query.RemoveItemAsync(item1);
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
			Assert.IsNotNull(output);
		}

		[TestMethod]
		public async Task ReplaceServiceTest_replaceSearchNull()
		{
			// When you search for nothing, there is nothing to replace 
			var output = await _metaReplace.Replace("/nothing.jpg", nameof(FileIndexItem.Tags), 
				null, "test", false);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,output[0].Status);
		}


		[TestMethod]
		public async Task ReplaceServiceTest_replace_LowerCaseTagName()
		{
			var item1 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test2.jpg",
				ParentDirectory = "/",
				Tags = "test1, !delete!, test"
			}); 
			
			var output = await _metaReplace.Replace("/test2.jpg",
				nameof(FileIndexItem.Tags).ToLowerInvariant(),"!delete!",string.Empty,false);

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,output[0].Status);
			Assert.AreEqual("test1, test",output[0].Tags);
			
			await _query.RemoveItemAsync(item1);
		}

		[TestMethod]
		public async Task ReplaceServiceTest_Readonly()
		{
			var item0 = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/readonly",
				Tags = "!delete!"
			});
			
			var output = await _metaReplace.Replace("/readonly/test.jpg",
				nameof(FileIndexItem.Tags),"!delete!",null,false);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, output.FirstOrDefault().Status);
			
			await _query.RemoveItemAsync(item0);
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
