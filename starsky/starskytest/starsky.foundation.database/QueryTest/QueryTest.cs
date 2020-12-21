using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskycore.Attributes;

namespace starskytest.starsky.foundation.database.QueryTest
{
    [TestClass]
    public class QueryTest
    {
        public QueryTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            _memoryCache = provider.GetService<IMemoryCache>();
            var serviceScope = CreateNewScope();
            var scope = serviceScope.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _query = new Query(dbContext,_memoryCache, 
	            new AppSettings{Verbose = true}, serviceScope);
        }

        private IServiceScopeFactory CreateNewScope()
        {
	        var services = new ServiceCollection();
	        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryTest)));
	        var serviceProvider = services.BuildServiceProvider();
	        return serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        private readonly Query _query;

        private static FileIndexItem _insertSearchDatahiJpgInput;
        private static FileIndexItem _insertSearchDatahi2JpgInput;
        private static FileIndexItem _insertSearchDatahi3JpgInput;
        private static FileIndexItem _insertSearchDatahi4JpgInput;
        private static FileIndexItem _insertSearchDatahi2SubfolderJpgInput;
        private readonly IMemoryCache _memoryCache;

        private void InsertSearchData()
        {
            if (string.IsNullOrEmpty(_query.GetSubPathByHash("09876543456789")))
            {
                _insertSearchDatahiJpgInput = _query.AddItem(new FileIndexItem
                {
                    FileName = "hi.jpg",
                    ParentDirectory = "/basic",
                    FileHash = "09876543456789",
                    ColorClass = ColorClassParser.Color.Winner, // 1
                    Tags = "",
                    Title = "",
                    IsDirectory = false
                });
                
                _insertSearchDatahi2JpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi2.jpg",
                    Tags = "!delete!",
                    ParentDirectory = "/basic",
                    IsDirectory = false
                });
            
                _insertSearchDatahi3JpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi3.jpg",
                    ParentDirectory = "/basic",
                    ColorClass = ColorClassParser.Color.Trash, // 9
                    IsDirectory = false
                });
            
                _insertSearchDatahi4JpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi4.jpg",
                    ParentDirectory = "/basic",
                    ColorClass = ColorClassParser.Color.Winner, // 1
                    IsDirectory = false
                });
            
                _insertSearchDatahi2SubfolderJpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi2.jpg",
                    ParentDirectory = "/basic/subfolder",
                    FileHash = "234567876543",
                    IsDirectory = false
                });
            }
        }

        [TestMethod]
        public void QueryForHomeDoesNotExist_Null()
        {
	        // remove if item exist
	        var homeItem = _query.SingleItem("/");
	        if ( homeItem != null )
	        {
		        _query.RemoveItem(homeItem.FileIndexItem);
		        
		        // Query again if needed
		        homeItem = _query.SingleItem("/");
	        }
	        
	        Assert.AreEqual(null,homeItem);
        }

        
        [TestMethod]
        public void QueryForHome()
        {
	        var item = _query.AddItem(new FileIndexItem("/"));
	        var home = _query.SingleItem("/").FileIndexItem;
	        Assert.AreEqual("/",home.FilePath);
	        _query.RemoveItem(item);
        }
        
        [TestMethod]
        public void QueryAddSingleItemHiJpgOutputTest()
        {
            InsertSearchData();
            var hiJpgOutput = _query.SingleItem(_insertSearchDatahiJpgInput.FilePath).FileIndexItem;

            Console.WriteLine(_insertSearchDatahiJpgInput.FileHash);
            Console.WriteLine(hiJpgOutput.FileHash);

            Assert.AreEqual(_insertSearchDatahiJpgInput.FileHash, hiJpgOutput.FileHash);
            
            // other api Get Object By FilePath
            hiJpgOutput = _query.GetObjectByFilePath(_insertSearchDatahiJpgInput.FilePath);
            Assert.AreEqual(_insertSearchDatahiJpgInput.FilePath, hiJpgOutput.FilePath);
        }
        
        /// <summary>
        ///  Item exist but not in folder cache, it now add this item to cache #228 
        /// </summary>
        [TestMethod]
        public void SingleItem_ItemExistInDbButNotInFolderCache()
        {
	        _query.AddItem(new FileIndexItem("/cache_test")
	        {
		        IsDirectory = true
	        });
	        var existingItem = new FileIndexItem("/cache_test/test.jpg");
	        _query.AddItem(existingItem);
	        _query.AddCacheParentItem("/cache_test", new List<FileIndexItem>{existingItem});
	        const string newItem = "/cache_test/test2.jpg";
	        _query.AddItem(new FileIndexItem(newItem));

	        var queryResult = _query.SingleItem(newItem);
	        Assert.IsNotNull(queryResult);
			Assert.AreEqual(newItem, queryResult.FileIndexItem.FilePath);

			foreach ( var items in _query.GetAllRecursive("/cache_test") )
			{
				_query.RemoveItem(items);
			}
        }

        
        [TestMethod]
        public async Task GetAllFilesAsync_Disposed()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new Query(dbContext,_memoryCache, 
		        new AppSettings{Verbose = true}, serviceScope);
		        
	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/") {IsDirectory = true});
	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/test.jpg"));
	        await dbContext.SaveChangesAsync();

	        // And dispose
	        await dbContext.DisposeAsync();
	        
	        var items = await query.GetAllFilesAsync("/");

	        Assert.AreEqual("/test.jpg", items[0].FilePath);
	        Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[0].Status);
        }

        [TestMethod]
        public async Task GetAllRecursiveAsync_GetResult()
        {
	        var appSettings = new AppSettings
	        {
		        DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
	        };
	        var dbContext = new SetupDatabaseTypes(appSettings, null).BuilderDbFactory();
	        var query = new Query(dbContext);

	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllRecursiveAsync") {IsDirectory = true});
	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllRecursiveAsync/test") {IsDirectory = true});
	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllRecursiveAsync/test.jpg"));
	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllRecursiveAsync/test/test.jpg"));
	        await dbContext.SaveChangesAsync();
	        
	        var items = await query.GetAllRecursiveAsync("/GetAllRecursiveAsync");

	        Assert.AreEqual(3,items.Count);
	        Assert.AreEqual("/GetAllRecursiveAsync/test", items[0].FilePath);
	        Assert.AreEqual("/GetAllRecursiveAsync/test.jpg", items[1].FilePath);
	        Assert.AreEqual("/GetAllRecursiveAsync/test/test.jpg", items[2].FilePath);

	        Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[0].Status);
	        Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[1].Status);
	        Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[2].Status);
        }
        
        [TestMethod]
        public async Task GetAllRecursiveAsync_Disposed()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new Query(dbContext,_memoryCache, 
		        new AppSettings{Verbose = true}, serviceScope);
		        
	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/gar") {IsDirectory = true});
	        await dbContext.FileIndex.AddAsync(new FileIndexItem("/gar/test.jpg"));
	        await dbContext.SaveChangesAsync();

	        // And dispose
	        await dbContext.DisposeAsync();
	        
	        var items = await query.GetAllRecursiveAsync("/gar");

	        Assert.AreEqual(1,items.Count);
	        Assert.AreEqual("/gar/test.jpg", items[0].FilePath);
	        Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[0].Status);
        }
        
        [TestMethod]
        public void QueryAddSingleItemGetAllRecursiveTest()
        {
            InsertSearchData();

            // GetAllRecursive
            var getAllRecursiveExpectedResult123 = new List<FileIndexItem>
            {
                _insertSearchDatahiJpgInput,
                _insertSearchDatahi2JpgInput,
                _insertSearchDatahi2SubfolderJpgInput,
                _insertSearchDatahi3JpgInput,
                _insertSearchDatahi4JpgInput
            }.OrderBy(p => p.FileName).ToList();
            
            var getAllRecursive123 = _query.GetAllRecursive()
                .Where(p => p.FilePath.Contains("/basic"))
                .OrderBy(p => p.FileName).ToList();

            Assert.AreEqual(getAllRecursive123.Count,getAllRecursiveExpectedResult123.Count);
            
            CollectionAssert.AreEqual(getAllRecursive123.Select(p => p.FileHash).ToList(), 
                getAllRecursiveExpectedResult123.Select(p => p.FileHash).ToList());
        }
        
        [TestMethod]
        public void QueryAddSingleItemGetAllRecursiveTest_DisposedItem()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new global::starsky.foundation.database.Query.Query(dbContext,_memoryCache, new AppSettings(), serviceScope);
	        
	        // item sub folder
	        var item = new FileIndexItem("/test_1231331/sub/test_0191919.jpg");
	        dbContext.FileIndex.Add(item);
	        dbContext.SaveChanges();
	        
	        // normal item
	        var item2 = new FileIndexItem("/test_1231331/test_0191919.jpg");
	        dbContext.FileIndex.Add(item2);
	        dbContext.SaveChanges();
	        
	        // Important to dispose!
	        dbContext.Dispose();

	        item.Tags = "test";
	        query.UpdateItem(item);

	        var getItem = query.GetAllRecursive("/test_1231331");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.FirstOrDefault().Tags);

	        query.RemoveItem(getItem.FirstOrDefault());
        }

        [TestMethod]
        public void QueryAddSingleItemGetItemByHashTest()
        {
            InsertSearchData();
            // GetSubPathByHash
            // See above for objects
            Assert.AreEqual("/basic/hi.jpg", _query.GetSubPathByHash("09876543456789"));
        }

        [TestMethod]
        public void QueryAddSingleItemNextWinnerTest()
        {
            InsertSearchData();
            // Next Winner
            var colorClassActiveList = new FileIndexItem().GetColorClassList("1");
            var next = _query.SingleItem("/basic/hi.jpg", colorClassActiveList);
            Assert.AreEqual("/basic/hi4.jpg", next.RelativeObjects.NextFilePath);
        }

        [TestMethod]
        public void QueryAddSingleItemPrevWinnerTest()
        {       
            InsertSearchData();
            // Prev Winner
            var colorClassActiveList = new FileIndexItem().GetColorClassList("1");
            var prev = _query.SingleItem("/basic/hi4.jpg", colorClassActiveList).RelativeObjects.PrevFilePath;
            Assert.AreEqual("/basic/hi.jpg", prev);
        }
        
        [TestMethod]
        public void QueryAddSingleItemDeletedStatus()
        {       
	        InsertSearchData();
	        var status = _query.SingleItem("/basic/hi2.jpg").FileIndexItem.Status;
	        Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, status);
        }


        [TestMethod]
        [ExcludeFromCoverage]
        public void QueryFolder_DisplayFileFoldersTest()
        {
            var hiJpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi.jpg",
                ParentDirectory = "/display", // without slash
                FileHash = "123458465522",
                ColorClass = ColorClassParser.Color.Winner // 1
            });
            
            var hi3JpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi3.jpg",
                ParentDirectory = "/display", // without slash
                FileHash = "78539048765",
                ColorClass = ColorClassParser.Color.Extras
            });
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "hi2.jpg",
                ParentDirectory = "/display",
                FileHash = "98765432123456",
                Tags = "!delete!"
            });
            
            // All Color Classes
            var getDisplayExpectedResult = new List<FileIndexItem> {hiJpgInput,hi3JpgInput};
            var getDisplay = _query.DisplayFileFolders("/display").ToList();
            
            CollectionAssert.AreEqual(getDisplayExpectedResult,getDisplay);
         
            // Compare filter
            var getDisplayExpectedResultSuperior = new List<FileIndexItem> {hiJpgInput};
            var colorClassActiveList = new FileIndexItem().GetColorClassList("1");
                
            var getDisplaySuperior = _query.DisplayFileFolders("/display",colorClassActiveList).ToList();
           
            CollectionAssert.AreEqual(getDisplayExpectedResultSuperior,getDisplaySuperior);

            // This feature is normal used for folders, for now it is done on files
            // Hi3.jpg Previous -- all mode
            var releative = _query.GetNextPrevInFolder("/display/hi3.jpg");
            
            // Folders ignore deleted items
            Assert.AreEqual(releative.PrevFilePath,"/display/hi2.jpg");
            Assert.AreEqual(releative.NextFilePath,null);

            // Next  Relative -- all mode
            var releative2 = _query.GetNextPrevInFolder("/display/hi.jpg");
            
            Assert.AreEqual(releative2.NextFilePath,"/display/hi2.jpg");
            Assert.AreEqual(releative2.PrevFilePath,null);
        }

        [TestMethod]
        public void QueryFolder_NextPrevDuplicates()
        {
	        var folder01 =  _query.AddItem(new FileIndexItem("/test_duplicate_01"){IsDirectory = true});
	        var folder02 =  _query.AddItem(new FileIndexItem("/test_duplicate_02"){IsDirectory = true});
	        var folder02duplicate =  _query.AddItem(new FileIndexItem("/test_duplicate_02"){IsDirectory = true});
	        var folder03 =  _query.AddItem(new FileIndexItem("/test_duplicate_03"){IsDirectory = true});

	        var result = _query.GetNextPrevInFolder("/test_duplicate_02");
			Assert.AreEqual("/test_duplicate_01",result.PrevFilePath);
			Assert.AreEqual("/test_duplicate_03",result.PrevFilePath);

			_query.RemoveItem(folder01);
			_query.RemoveItem(folder02);
			_query.RemoveItem(folder02duplicate);
			_query.RemoveItem(folder03);
        }

        [TestMethod]
        public void QueryDisplayFileFolders_Duplicates_Test()
        {
	        var image0 =  _query.AddItem(new FileIndexItem
	        {
		        FileName = "0.jpg",
		        ParentDirectory = "/duplicates_test", 
		        FileHash = "45782347832",
		        ColorClass = ColorClassParser.Color.Winner // 1
	        });
	        
	        var image1 =  _query.AddItem(new FileIndexItem
	        {
		        FileName = "1.jpg",
		        ParentDirectory = "/duplicates_test", 
		        FileHash = "123458465522",
		        ColorClass = ColorClassParser.Color.Winner // 1
	        });
            
	        var image1Duplicate =  _query.AddItem(new FileIndexItem
	        {
		        FileName = "1.jpg",
		        ParentDirectory = "/duplicates_test", 
		        FileHash = "821847217",
		        ColorClass = ColorClassParser.Color.Winner // 1
	        });
            
	        var image2 = _query.AddItem(new FileIndexItem
	        {
		        FileName = "2.jpg",
		        ParentDirectory = "/duplicates_test",
		        FileHash = "98765432123456",
	        });

	        _query.AddRangeAsync(
		        new List<FileIndexItem> {image0, image1, image1Duplicate, image2});

	        var result = _query.QueryDisplayFileFolders("/duplicates_test");

	        Assert.AreEqual(3,result.Count);

	        Assert.AreEqual(image0, result[0]);
	        Assert.AreEqual(image1, result[1]);
	        Assert.AreEqual(image2, result[2]);
        }

        [TestMethod]
        public void QueryFolder_DisplayFileFoldersNoResultTest()
        {
            var getDisplay = _query.DisplayFileFolders("/12345678987654").ToList();
            Assert.AreEqual(0, getDisplay.Count);
        }
        
        
        [TestMethod]
        public void QueryFolder_DisplayFileFolders_OneItemInFolder_DisposedItem()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new global::starsky.foundation.database.Query.Query(dbContext,_memoryCache, new AppSettings(), serviceScope);
	        
	        var item = new FileIndexItem("/test_0191919/test_0191919.jpg");
	        dbContext.FileIndex.Add(item);
	        dbContext.SaveChanges();
	        
	        // Important to dispose!
	        dbContext.Dispose();

	        item.Tags = "test";
	        query.UpdateItem(item);

	        var getItem = query.DisplayFileFolders("/test_0191919");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.FirstOrDefault().Tags);

	        query.RemoveItem(getItem.FirstOrDefault());
        }

        [ExcludeFromCoverage]
        [TestMethod]
        public void BreadcrumbDetailViewTest()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "hi3.jpg",
                //FilePath = "/bread/hi3.jpg",
                ParentDirectory = "/bread", // without slash
                FileHash = "234565432",
                ColorClass = ColorClassParser.Color.Extras,
                IsDirectory = false
            });
            
            var exptectedOutput = new List<string>{"/","/bread"};
            var output = _query.SingleItem("/bread/hi3.jpg").Breadcrumb;
            CollectionAssert.AreEqual(exptectedOutput,output);
        }

        [TestMethod]
        public void BreadcrumbDetailViewPagViewTypeTest()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "hi4.jpg",
                //FilePath = "/bread/hi4.jpg",
                ParentDirectory = "/bread", // without slash
                FileHash = "23456543",
                ColorClass = ColorClassParser.Color.Extras,
                IsDirectory = false
            });
            
            // Used for react to get the context
            var pageTypeReact = _query.SingleItem("/bread/hi4.jpg").PageType;
            Assert.AreEqual("DetailView",pageTypeReact);
        }
        
        
        [TestMethod]
        public void QueryTest_NextFilePathCachingConflicts_Deleted()
        {
            // init items
            _query.AddItem(new FileIndexItem
            {
                FileName = "CachingDeleted_001.jpg",
                ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
                FileHash = "0987345678654345678",
                Tags = string.Empty,
                IsDirectory = false
            });
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "CachingDeleted_002.jpg",
                ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
                FileHash = "3456783456780987654",
                Tags = string.Empty,
                IsDirectory = false
            });
            
            var single001 = 
                _query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_001.jpg");
            Assert.AreEqual("/QueryTest_NextPrevCachingDeleted/CachingDeleted_002.jpg",
                single001.RelativeObjects.NextFilePath);

            var single002 =
                _query
                    .SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_002.jpg").FileIndexItem;
            single002.Tags = "!delete!";
            _query.UpdateItem(single002);
            
            // Request new; and check if content is updated in memory cache
            single001 = 
                _query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_001.jpg");
            Assert.AreEqual(null,single001.RelativeObjects.NextFilePath);
            
            // For avoiding conflicts when running multiple unit tests
            single001.FileIndexItem.Tags = "!delete!";
            _query.UpdateItem(single001.FileIndexItem);
            
        }

        [TestMethod]
        public void Query_UpdateItem_1_DisposedItem()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new global::starsky.foundation.database.Query.Query(dbContext,_memoryCache, 
		        new AppSettings{Verbose = true}, serviceScope);
	        
	        var item = new FileIndexItem("/test/010101.jpg");
	        dbContext.FileIndex.Add(item);
	        dbContext.SaveChanges();
	        
	        // Important to dispose!
	        dbContext.Dispose();

	        item.Tags = "test";
	        query.UpdateItem(item);

	        var getItem = query.GetObjectByFilePath("/test/010101.jpg");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.Tags);

	        query.RemoveItem(getItem);
        }

        [TestMethod]
        public void Query_GetObjectByFilePath_home()
        {
	        
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new global::starsky.foundation.database.Query.Query(dbContext,_memoryCache, 
		        new AppSettings{Verbose = true}, serviceScope);
	        query.AddItem(new FileIndexItem("/"));
	        
	        var item = query.GetObjectByFilePath("/");
	        Assert.IsNotNull(item);
	        Assert.AreEqual("/", item.FilePath);
	        Assert.AreEqual("/", item.FileName);
        }
        
        
        [TestMethod]
        public async Task Query_GetObjectByFilePathAsync_home()
        {
	        var dbItem = await _query.AddItemAsync(new FileIndexItem("/"));
	        
	        var item = await _query.GetObjectByFilePathAsync("/");
	        Assert.IsNotNull(item);
	        Assert.AreEqual("/", item.FilePath);
	        Assert.AreEqual("/", item.FileName);
	        
	        await _query.RemoveItemAsync(dbItem);
        }
        
        [TestMethod]
        public async Task Query_GetObjectByFilePathAsync_Disposed()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new global::starsky.foundation.database.Query.Query(dbContext,_memoryCache, 
		        new AppSettings{Verbose = true}, serviceScope);
	        var item = await query.AddItemAsync(new FileIndexItem("/GetObjectByFilePathAsync/test.jpg")
	        {
		        Tags = "hi"
	        });

	        // important to Dispose
	        await dbContext.DisposeAsync();

	        item.Tags = "test";
	        await query.UpdateItemAsync(item);
	        
	        var getItem = await query.GetObjectByFilePathAsync("/GetObjectByFilePathAsync/test.jpg");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("/GetObjectByFilePathAsync/test.jpg", getItem.FilePath);
	        Assert.AreEqual("test.jpg", getItem.FileName);
	        Assert.AreEqual("test", getItem.Tags);
        }

        [TestMethod]
        public void Query_UpdateItem_Multiple_DisposedItem()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new global::starsky.foundation.database.Query.Query(dbContext,_memoryCache, new AppSettings(), serviceScope);
	        
	        var item = new FileIndexItem("/test/010101.jpg");
	        dbContext.FileIndex.Add(item);
	        dbContext.SaveChanges();
	        
	        // Important to dispose!
	        dbContext.Dispose();

	        item.Tags = "test";
	        query.UpdateItem(new List<FileIndexItem>{item});

	        var getItem = query.GetObjectByFilePath("/test/010101.jpg");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.Tags);

	        query.RemoveItem(getItem);
        }

        [TestMethod]
        public async Task UpdateItemAsync_Single()
        {
	        var item2 = new FileIndexItem("/test2.jpg");
	        await _query.AddItemAsync(item2);
	        
	        item2.Tags = "test";
			await _query.UpdateItemAsync(item2);

	        var getItem = await _query.GetObjectByFilePathAsync("/test2.jpg");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.Tags);

	        await _query.RemoveItemAsync(getItem);
        }

        [TestMethod]
        public async Task UpdateItemAsync_Single_DisposedItem()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new global::starsky.foundation.database.Query.Query(dbContext,_memoryCache, new AppSettings(), serviceScope);
	        
	        var item = new FileIndexItem("/test/010101.jpg");
	        await dbContext.FileIndex.AddAsync(item);
	        await dbContext.SaveChangesAsync();
	        
	        // Important to dispose!
	        await dbContext.DisposeAsync();

	        item.Tags = "test";
	        await query.UpdateItemAsync(item);

	        var getItem = await query.GetObjectByFilePathAsync("/test/010101.jpg");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.Tags);

	        await query.RemoveItemAsync(getItem);
        }
        
        [TestMethod]
        public async Task UpdateItemAsync_Multiple()
        {
	        var item1 = new FileIndexItem("/test24f1s54.jpg");
	        var item2 = new FileIndexItem("/test885828.jpg");

	        await _query.AddItemAsync(item1);
	        await _query.AddItemAsync(item2);

	        item1.Tags = "test";
	        item2.Tags = "test";

	        await _query.UpdateItemAsync(new List<FileIndexItem>{item1,item2});

	        var getItem = await _query.GetObjectByFilePathAsync("/test24f1s54.jpg");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.Tags);

	        var getItem2 = await _query.GetObjectByFilePathAsync("/test885828.jpg");
	        Assert.IsNotNull(getItem2);
	        Assert.AreEqual("test", getItem2.Tags);

	        await _query.RemoveItemAsync(getItem);
	        await _query.RemoveItemAsync(getItem2);
        }
        
        [TestMethod]
        public async Task UpdateItemAsync_Multiple_DisposedItem()
        {
	        var serviceScope = CreateNewScope();
	        var scope = serviceScope.CreateScope();
	        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	        var query = new Query(dbContext,_memoryCache, new AppSettings(), serviceScope);
	        
	        var item = new FileIndexItem("/test/8284574.jpg");
	        await dbContext.FileIndex.AddAsync(item);
	        await dbContext.SaveChangesAsync();
	        
	        var item2 = new FileIndexItem("/test/8284575.jpg");
	        await dbContext.FileIndex.AddAsync(item2);
	        await dbContext.SaveChangesAsync();
	        
	        // Important to dispose!
	        await dbContext.DisposeAsync();

	        item.Tags = "test";
	        item2.Tags = "test";

	        await query.UpdateItemAsync(new List<FileIndexItem>{item,item2});

	        var getItem = await query.GetObjectByFilePathAsync("/test/8284574.jpg");
	        Assert.IsNotNull(getItem);
	        Assert.AreEqual("test", getItem.Tags);

	        var getItem2 = await query.GetObjectByFilePathAsync("/test/8284575.jpg");
	        Assert.IsNotNull(getItem2);
	        Assert.AreEqual("test", getItem2.Tags);
	        
	        await query.RemoveItemAsync(getItem);
	        await query.RemoveItemAsync(getItem2);
        }

        [TestMethod]
        public void QueryTest_PrevFilePathCachingConflicts_Deleted()
        {
            // For previous item check if caching has no conflicts
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "CachingDeleted_003.jpg",
                ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
                FileHash = "56787654",
                Tags = string.Empty,
                IsDirectory = false
            });
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "CachingDeleted_004.jpg",
                ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
                FileHash = "98765467876",
                Tags = string.Empty,
                IsDirectory = false
            });
            
            // For previous item; 
            var single004 = 
                _query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_004.jpg");
            Assert.AreEqual("/QueryTest_NextPrevCachingDeleted/CachingDeleted_003.jpg",
                single004.RelativeObjects.PrevFilePath);

            var single003 =
                _query
                    .SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_003.jpg").FileIndexItem;
            single003.Tags = "!delete!";
            _query.UpdateItem(single003);
            
            // Request new; item must be updated in cache
            single004 = 
                _query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_004.jpg");
            Assert.AreEqual(null,single004.RelativeObjects.PrevFilePath);
            
            // For avoiding conflicts when running multiple unit tests
            single004.FileIndexItem.Tags = "!delete!";
            _query.UpdateItem(single004.FileIndexItem);
            
        }


        [TestMethod]
        public void QueryTest_TestPreviousFileHash()
        {
	        // For previous item check if caching has no conflicts

	        _query.AddItem(new FileIndexItem
	        {
		        FileName = "001.jpg",
		        ParentDirectory = "/QueryTest_prev_hash",
		        FileHash = "09457777777",
		        Tags = string.Empty,
		        IsDirectory = false
	        });

	        _query.AddItem(new FileIndexItem
	        {
		        FileName = "002.jpg",
		        ParentDirectory = "/QueryTest_prev_hash",
		        FileHash = "09457847385873",
		        Tags = string.Empty,
		        IsDirectory = false
	        });

	        // For previous item; 
	        var single004 =
		        _query.SingleItem("/QueryTest_prev_hash/002.jpg");
	        
	        // check hash
	        Assert.AreEqual("09457777777",
		        single004.RelativeObjects.PrevHash);
	        
	        // check path
	        Assert.AreEqual("/QueryTest_prev_hash/001.jpg",
		        single004.RelativeObjects.PrevFilePath);
        }
        
        
        [TestMethod]
        public void QueryTest_TestNextFileHash()
        {
	        // For NEXT item check if caching has no conflicts

	        _query.AddItem(new FileIndexItem
	        {
		        FileName = "001.jpg",
		        ParentDirectory = "/QueryTest_next_hash",
		        FileHash = "09457777777",
		        Tags = string.Empty,
		        IsDirectory = false
	        });

	        _query.AddItem(new FileIndexItem
	        {
		        FileName = "002.jpg",
		        ParentDirectory = "/QueryTest_next_hash",
		        FileHash = "09457847385873",
		        Tags = string.Empty,
		        IsDirectory = false
	        });

	        // For previous item; 
	        var single004 =
		        _query.SingleItem("/QueryTest_next_hash/001.jpg");
	        
	        // check hash
	        Assert.AreEqual("09457847385873",
		        single004.RelativeObjects.NextHash);
	        
	        // check path
	        Assert.AreEqual("/QueryTest_next_hash/002.jpg",
		        single004.RelativeObjects.NextFilePath);
        }

        [TestMethod]
        public void QueryTest_CachingDirectoryConflicts_CheckIfContentIsInCacheUpdated()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "CachingDeleted_001.jpg",
                ParentDirectory = "/CheckIfContentIsInCacheUpdated",
                FileHash = "567897",
                IsDirectory = false
            });
            
            // trigger caching
            _query.DisplayFileFolders("/CheckIfContentIsInCacheUpdated");
            
            var cachingDeleted001 = 
                _query.SingleItem("/CheckIfContentIsInCacheUpdated/CachingDeleted_001.jpg").FileIndexItem;
            cachingDeleted001.Tags = "#";
            _query.UpdateItem(cachingDeleted001);
            var cachingDeleted001Update = 
                _query.SingleItem("/CheckIfContentIsInCacheUpdated/CachingDeleted_001.jpg").FileIndexItem;
            Assert.AreEqual("#", cachingDeleted001Update.Tags);
            Assert.AreNotEqual(string.Empty, cachingDeleted001Update.Tags);
            // AreNotEqual: When it item used cache  it will return string.Emthy
            
        }

        [TestMethod]
        public void QueryFolder_Add_And_UpdateFolderCache_UpdateCacheItemTest()
        {
            // Add folder to cache normally done by: CacheQueryDisplayFileFolders
            _memoryCache.Set("List`1_/", new List<FileIndexItem>(), 
	            new TimeSpan(1,0,0));
            // "List`1_" is from CachingDbName
            
            var item = new FileIndexItem {Id = 400, FileName = "cache"};
            _query.AddCacheItem(item);
            
            var item1 = new FileIndexItem {Id = 400, Tags = "hi", FileName = "cache"};
            _query.CacheUpdateItem(new List<FileIndexItem>{item1});

            _memoryCache.TryGetValue("List`1_/", out var objectFileFolders);
            var displayFileFolders = (List<FileIndexItem>) objectFileFolders;

            Assert.AreEqual("hi",displayFileFolders.FirstOrDefault(p => p.FileName == "cache").Tags);
	        
	        Assert.AreEqual(1,displayFileFolders.Count(p => p.FileName == "cache"));
        }

        [TestMethod]
        public void DisplayFileFolders_StackCollection()
        {
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "StackCollection001.jpg",
                ParentDirectory = "/StackCollection",
                FileHash = "9876789",
                Tags = string.Empty,
                IsDirectory = false
            });
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "StackCollection001.dng",
                ParentDirectory = "/StackCollection",
                FileHash = "9876789",
                Tags = string.Empty,
                IsDirectory = false
            });


            var dp1 = _query.DisplayFileFolders("/StackCollection");
            Assert.AreEqual(1,dp1.Count());
            
            var dp2 = _query.DisplayFileFolders("/StackCollection",null,false);
            Assert.AreEqual(2,dp2.Count());
        }

        [TestMethod]
        public void Query_updateStatusContentList()
        {
            // for updateing multiple items
            var toupdate = new List<FileIndexItem>{new FileIndexItem
            {
                Tags = "test",
                FileName = "3456784567890987654.jpg",
                ParentDirectory = "/3456784567890987654",
                FileHash = "3456784567890987654"
            }};
            _query.AddItem(toupdate.FirstOrDefault());

            foreach (var item in toupdate)
            {
                item.Tags = "updated";
            }
            _query.UpdateItem(toupdate);

            var fileObjectByFilePath = _query.GetObjectByFilePath("/3456784567890987654/3456784567890987654.jpg");
            Assert.AreEqual("updated",fileObjectByFilePath.Tags);
        }
        
        [TestMethod]
        public async Task Query_updateStatusContentList_Async()
        {
	        // for updateing multiple items
	        var toupdate = new List<FileIndexItem>{new FileIndexItem
	        {
		        Tags = "test",
		        FileName = "9278521.jpg",
		        ParentDirectory = "/8118",
		        FileHash = "3456784567890987654"
	        }};
	        await _query.AddItemAsync(toupdate.FirstOrDefault());

	        foreach (var item in toupdate)
	        {
		        item.Tags = "updated";
	        }
	        _query.UpdateItem(toupdate);

	        var fileObjectByFilePath = await _query.GetObjectByFilePathAsync("/8118/9278521.jpg");
	        Assert.AreEqual("updated",fileObjectByFilePath.Tags);
        }


	    [TestMethod]
	    public void Query_IsCacheEnabled_True()
	    {
		    Assert.AreEqual(true, _query.IsCacheEnabled());
	    }

	    [TestMethod]
	    public async Task AddItemAsync()
	    {
		    var item = await _query.AddItemAsync(new FileIndexItem("/test/test.jpg"));
		    
		    var result = _query.SingleItem("/test/test.jpg");

		    Assert.IsNotNull(result.FileIndexItem);
		    Assert.AreEqual("/test/test.jpg", result.FileIndexItem.FilePath);
		    
		    await _query.RemoveItemAsync(item);
	    }

	    [TestMethod]
	    public async Task AddItemAsync_Disposed()
	    {
		    var serviceScope = CreateNewScope();
		    var scope = serviceScope.CreateScope();
		    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		    var query = new Query(dbContext,_memoryCache, new AppSettings(), serviceScope);

		    await dbContext.DisposeAsync();
		    await query.AddItemAsync(new FileIndexItem("/test982.jpg")
		    {
			    Tags = "test"
		    });
		    
		    var dbContext2 = new InjectServiceScope(serviceScope).Context();
		    var itemItShouldContain = await dbContext2.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test982.jpg");
		    Assert.IsNotNull(itemItShouldContain);
		    Assert.AreEqual("test", itemItShouldContain.Tags);
	    }

	    [TestMethod]
	    public async Task RemoveItemAsync()
	    {
		    var serviceScope = CreateNewScope();
		    var scope = serviceScope.CreateScope();
		    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		    var query = new Query(dbContext,_memoryCache, new AppSettings(), serviceScope);

		    await dbContext.FileIndex.AddAsync(new FileIndexItem("/test44.jpg"));
		    await dbContext.SaveChangesAsync();

		    var item = await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");
		    await query.RemoveItemAsync(item);
		    
		    var itemItShouldBeNull = await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");
		    Assert.IsNull(itemItShouldBeNull);
	    }
	    
	    [TestMethod]
	    public async Task RemoveItemAsync_Disposed()
	    {
		    var serviceScope = CreateNewScope();
		    var scope = serviceScope.CreateScope();
		    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		    var query = new Query(dbContext,_memoryCache, new AppSettings(), serviceScope);

		    await dbContext.FileIndex.AddAsync(new FileIndexItem("/test44.jpg"));
		    await dbContext.SaveChangesAsync();

		    var item = await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");

		    await dbContext.DisposeAsync();

		    await query.RemoveItemAsync(item);

		    var dbContext2 = new InjectServiceScope(serviceScope).Context();
		    var itemItShouldBeNull = await dbContext2.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");
		    Assert.IsNull(itemItShouldBeNull);
	    }

	    [TestMethod]
	    public void RemoveCacheItem_ShouldKeepOne()
	    {
		    var dirPath = "/__cache__remove_test";
		    var demoItems =
			    new List<FileIndexItem>
			    {
				    new FileIndexItem(dirPath+ "/01.jpg"),
				    new FileIndexItem(dirPath+ "/02.jpg"),
				    new FileIndexItem(dirPath+ "/03.jpg")
			    };
		    
		    _query.AddCacheParentItem(dirPath,demoItems);
		    
		    _query.RemoveCacheItem(new List<FileIndexItem>{demoItems[0], demoItems[1]});

		    var result = _query.DisplayFileFolders(dirPath).ToList();
		    Assert.AreEqual(1,result.Count);
		    Assert.AreEqual(dirPath+ "/03.jpg",result[0].FilePath);
	    }
    }
}
