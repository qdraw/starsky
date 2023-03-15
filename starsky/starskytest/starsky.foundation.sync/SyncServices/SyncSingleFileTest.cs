using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public sealed class SyncSingleFileTest
	{
		private readonly IStorage _iStorageFake;
		private readonly DateTime _lastEditedDateTime;

		public SyncSingleFileTest()
		{
			_lastEditedDateTime = new DateTime(2020, 02, 02);
			_iStorageFake = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg","/color_class_test.jpg", "/status_deleted.jpg"},
				new List<byte[]>{CreateAnImageNoExif.Bytes, 
					CreateAnImageColorClass.Bytes, CreateAnImageStatusDeleted.Bytes}, new List<DateTime>
				{
					_lastEditedDateTime,
					_lastEditedDateTime,
					_lastEditedDateTime
				});
		}

		[TestMethod]
		public async Task SingleFile_FileType_NotSupported()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result = (await sync.SingleFile("/non_exist.ext")).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result?.Status);
		}
		
		[TestMethod]
		public async Task SingleFile_ImageFormat_Corrupt()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/corrupt.jpg"},
				new List<byte[]>{new byte[5]});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				storage, null, new FakeIWebLogger());
			var result = (await sync.SingleFile("/corrupt.jpg")).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result?.Status);
		}
		
		[TestMethod]
		public async Task SingleFile_AddNewFile()
		{
			const string filePath = "/test.jpg";
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			var result = (await sync.SingleFile(filePath)).FirstOrDefault();
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);
			
			// should add files to db
			var detailView = fakeQuery.SingleItem(filePath);
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg", fileIndexItem?.FilePath);
			
			// should not duplicate add items
			var count= (await fakeQuery.GetAllFilesAsync("/")).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
		}
		
		[TestMethod]
		public async Task SingleFile_AddNewFile_StatusDeleted()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			var result = (await sync.SingleFile("/status_deleted.jpg")).FirstOrDefault();
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, result?.Status);
		}
	
	
		[TestMethod]
		public async Task SingleFile_AddNewFile_WithParentFolders()
		{
			var iStorageFake = new FakeIStorage(new List<string>{"/", "/level", "/level/deep"},
				new List<string>{"/level/deep/test.jpg"},
				new List<byte[]>{CreateAnImageNoExif.Bytes});
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				iStorageFake, null, new FakeIWebLogger());
			await sync.SingleFile("/level/deep/test.jpg");

			var detailView = fakeQuery.SingleItem("/level/deep/test.jpg");
			// should add files to db
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/level/deep/test.jpg", fileIndexItem?.FilePath);
			
			// should not duplicate add items
			var count= (await fakeQuery.GetAllFilesAsync("/level/deep"))
				.Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			// should add parent items
			// root folder
			var rootFolder = fakeQuery.SingleItem("/")?.FileIndexItem;
			Assert.IsNotNull(rootFolder);
			Assert.AreEqual(true,rootFolder.IsDirectory);

			// sub folder 'level folder
			var levelFolder = fakeQuery.SingleItem("/level")?.FileIndexItem;
			Assert.IsNotNull(levelFolder);
			Assert.AreEqual(true,levelFolder.IsDirectory);

			// sub folder 'level deep folder
			var levelDeepFolder = fakeQuery.SingleItem("/level/deep")?.FileIndexItem;
			Assert.IsNotNull(levelDeepFolder);
			Assert.AreEqual(true,levelDeepFolder.IsDirectory);
		}

		[TestMethod]
		public async Task SingleFile_FileAlreadyExist_WithSameFileHash()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash,
					LastEdited = _lastEditedDateTime
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result = (await sync.SingleFile("/test.jpg")).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result?.Status);
			
			var count= (await fakeQuery.GetAllFilesAsync("/")).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg", fileIndexItem?.FilePath);
		}

		[TestMethod]
		public async Task SingleFile_FileAlreadyExist_WithSameFileHash_ShouldNotTrigger()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash,
					LastEdited = _lastEditedDateTime
				},
				new FileIndexItem("/")
				{
					IsDirectory = true
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());


			var isCalled = false;
			Task TestTask(List<FileIndexItem> item)
			{
				isCalled = true;
				return Task.CompletedTask;
			}
			
			await sync.SingleFile("/test.jpg",TestTask);

			Assert.IsFalse(isCalled);
		}
		
		
		[TestMethod]
		public async Task SingleFile_FileAlreadyExist_With_Same_LastEditedTime()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash,
					Size = _iStorageFake.Info("/test.jpg").Size, // < right byte size
					Tags = "the tags should not be updated", // <= the tags in /test.jpg is nothing,
					LastEdited = _lastEditedDateTime
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			var result = (await sync.SingleFile("/test.jpg")).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result?.Status);
			
			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
		}
		
		[TestMethod]
		public async Task SingleFile_FileAlreadyExist_With_Different_LastEditedTime()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash,
				Size =
					_iStorageFake.Info("/test.jpg").Size, // < right byte size
				Tags =
					"the tags should not be updated", // <= the tags in /test.jpg is nothing,
				LastEdited = new DateTime(1999, 01, 02)
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				item
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			var result = (await sync.SingleFile("/test.jpg")).FirstOrDefault();
			Assert.AreEqual(1, result?.LastChanged.Count(p => p == nameof(FileIndexItem.LastEdited)));

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);
			
			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
			Assert.AreEqual(_lastEditedDateTime, fileIndexItem?.LastEdited);
		}

		[TestMethod]
		public async Task SingleFile_FileAlreadyExist_With_Changed_FileHash()
		{
			const string currentFilePath = "/test_date2.jpg";
			_iStorageFake.FileCopy("/test.jpg", currentFilePath);
			(_iStorageFake as FakeIStorage)!.SetDateTime(currentFilePath,DateTime.UtcNow);
			
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync(currentFilePath);
				
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem(currentFilePath)
				{
					FileHash = "THIS_IS_THE_OLD_HASH",
					Size = 99999999 // % % % that's not the right size % % %
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result = (await sync.SingleFile(currentFilePath)).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);

			var count= (await fakeQuery.GetAllFilesAsync("/")).Count(p => p.FilePath == currentFilePath);
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem(currentFilePath);
			
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual(currentFilePath,fileIndexItem?.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem?.FileHash);
			
			// Should be around now-ish
			Assert.AreEqual(DateTime.UtcNow.Day, fileIndexItem?.LastEdited.Day);
			Assert.AreEqual(DateTime.UtcNow.Month, fileIndexItem?.LastEdited.Month);
			Assert.AreEqual(DateTime.UtcNow.Hour, fileIndexItem?.LastEdited.Hour);
			Assert.AreEqual(DateTime.UtcNow.Minute, fileIndexItem?.LastEdited.Minute);
		}

		[TestMethod]
		public async Task SingleFile_FileAlreadyExist_With_Changed_FileHash_ShouldTriggerDelegate()
		{
			_iStorageFake.FileCopy("/test.jpg", "/test_23456.jpg");
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test_23456.jpg")
				{
					FileHash = "THIS_IS_THE_OLD_HASH",
					Size = 99999999 // % % % that's not the right size % % %
				}
			});
			var isCalled = false;
			Task TestTask(List<FileIndexItem> _)
			{
				isCalled = true;
				return Task.CompletedTask;
			}
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			await sync.SingleFile("/test_23456.jpg",TestTask);
			
			Assert.IsTrue(isCalled);
		}
		
				
		[TestMethod]
		public async Task SingleFile_FileAlreadyExist_With_Different_LastEditedTime_AppSettingsIgnore()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash,
				Size =
					_iStorageFake.Info("/test.jpg").Size, // < right byte size
				Tags =
					"the tags should not be updated", // <= the tags in /test.jpg is nothing,
				LastEdited = new DateTime(1999, 01, 02),
				Status = FileIndexItem.ExifStatus.Ok
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				item
			});
			
			var sync = new SyncSingleFile(new AppSettings
				{
					SyncAlwaysUpdateLastEditedTime = false // <-- ignore due this setting
				}, fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			var result = (await sync.SingleFile("/test.jpg")).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result.Status);
			Assert.AreEqual(0, result.LastChanged.Count);
			Assert.AreEqual(0, result.LastChanged.Count(p => p == nameof(FileIndexItem.LastEdited)));

			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
		}
		
		[TestMethod]
		public async Task SingleItem_DbItem_Updated()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			// % % % % Enter item here % % % % % 
			var result = (await sync.SingleFile("/test.jpg",
				new List<FileIndexItem>{item})).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);

			var count= (await fakeQuery.GetAllFilesAsync("/")).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg",fileIndexItem?.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem?.FileHash);
		}
		
		// - - - - - - - - - - - - - - - - -  DB ITEMS - - - - - - - - - - - - - -
		
		[TestMethod]
		public async Task SingleFile_DbItem_FileAlreadyExist_WithSameFileHash()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash, LastEdited = _lastEditedDateTime
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				item
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			// % % % % Enter item here % % % % % 
			var result = (await sync.SingleFile("/test.jpg",
				new List<FileIndexItem>{item})).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result.Status);
			
			var count= (await fakeQuery.GetAllFilesAsync("/")).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg", fileIndexItem?.FilePath);
		}
		
		[TestMethod]
		public async Task SingleItem_DbItem_Null()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH"
			};
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			// % % % % Enter item here % % % % % 
			var result = (await sync.SingleFile("/test.jpg",
				null as List<FileIndexItem>)).FirstOrDefault();
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result.Status);

			var count= (await fakeQuery.GetAllFilesAsync("/")).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg",fileIndexItem?.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem?.FileHash);
		}
		
		
				
		[TestMethod]
		public async Task SingleFile_DbItem_FileAlreadyExist_With_Different_LastEditedTime()
		{
			const string filePath = "/FileAlreadyExist_With_Different_LastEditedTime.jpg";
			_iStorageFake.FileCopy("/test.jpg", filePath);
			( _iStorageFake as FakeIStorage )?.SetDateTime(filePath,
				_lastEditedDateTime);

			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync(filePath);

			var item = new FileIndexItem(filePath)
			{
				FileHash = fileHash,
				Size =
					_iStorageFake.Info(filePath).Size, // < right byte size
				Tags =
					"the tags should not be updated", // <= the tags in /test.jpg is nothing,
				LastEdited = new DateTime(1999, 01, 02)
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				item
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null!, new FakeIWebLogger());
			
			var result = (await sync.SingleFile(filePath,
				new List<FileIndexItem>{item})).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);
			Assert.AreEqual(1, result?.LastChanged.Count(p => p == nameof(FileIndexItem.LastEdited)));
			
			var fileIndexItem = fakeQuery.SingleItem(filePath)?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
			Assert.AreEqual(_lastEditedDateTime, fileIndexItem?.LastEdited);
		}
		
		[TestMethod]
		public async Task SingleFile_DbItem_FileAlreadyExist_With_Different_LastEditedTime_AndDeleted()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash,
				Size =
					_iStorageFake.Info("/test.jpg").Size, // < right byte size
				Tags =
					$"the tags should not be updated, {TrashKeyword.TrashKeywordString}", // <= the tags in /test.jpg is nothing,
				LastEdited = new DateTime(1999, 01, 02)
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				item
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			var result = (await sync.SingleFile("/test.jpg",
				new List<FileIndexItem>{item})).FirstOrDefault();
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, result.Status);
			Assert.AreEqual(1, result.LastChanged.Count(p => p == nameof(FileIndexItem.LastEdited)));
			
			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual($"the tags should not be updated, {TrashKeyword.TrashKeywordString}", fileIndexItem?.Tags);
			Assert.AreEqual(_lastEditedDateTime, fileIndexItem?.LastEdited);
		}
		
		[TestMethod]
		public async Task SingleFile_DbItem_FileAlreadyExist_With_Different_LastEditedTime_AppSettingsIgnore()
		{
			const string filePath = "/fileAlreadyExist_With_Different_LastEditedTime.jpg";
			_iStorageFake.FileCopy("/test.jpg", filePath);
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync(filePath);

			var item = new FileIndexItem(filePath)
			{
				FileHash = fileHash,
				Size =
					_iStorageFake.Info(filePath).Size, // < right byte size
				Tags =
					"the tags should not be updated", // <= the tags in /test.jpg is nothing,
				LastEdited = new DateTime(1999, 01, 02),
				Status = FileIndexItem.ExifStatus.Ok
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				item
			});
			
			var sync = new SyncSingleFile(new AppSettings
				{
					SyncAlwaysUpdateLastEditedTime = false // <-- ignore due this setting
				}, fakeQuery,
				_iStorageFake, null!, new FakeIWebLogger());
			
			var result = (await sync.SingleFile(filePath,
				new List<FileIndexItem>{item})).FirstOrDefault();

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);
			Assert.AreEqual(0, result?.LastChanged.Count);
			Assert.AreEqual(0, result?.LastChanged.Count(p => p == nameof(FileIndexItem.LastEdited)));

			var fileIndexItem = fakeQuery.SingleItem(filePath)?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
		}
		
		[TestMethod]
		public async Task SingleItem_SidecarFileTest()
		{
			const string filePathRaw = "/SingleItem_SidecarFileTest.dng";
			const string filePathXmp = "/SingleItem_SidecarFileTest.xmp";
			var lastEdited = DateTime.Now;

			// It should update the Sidecar field when a sidecar file is add to the directory
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{filePathRaw, filePathXmp}, new List<byte[]>{
					CreateAnImageNoExif.Bytes,
					CreateAnXmp.Bytes}, new List<DateTime>{lastEdited, lastEdited});
			
			var (fileHashRaw, _) = await new FileHash(storage).GetHashCodeAsync(filePathRaw);

			var item = new FileIndexItem(filePathRaw)
			{
				FileHash = fileHashRaw, // < right file hash
				Size = _iStorageFake.Info(filePathRaw).Size, // < right byte size
				LastEdited = lastEdited,
				Tags = "before",
				ColorClass = ColorClassParser.Color.None
			};
			
			var item2 = new FileIndexItem(filePathXmp)
			{
				FileHash = "xmpHasChanged", 
				Size = _iStorageFake.Info(filePathXmp).Size, 
				LastEdited = new DateTime(2000,01, 01),
			};
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item,item2});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				storage, null!, new FakeIWebLogger());
			
			await sync.SingleFile(filePathXmp);
			
			var fileIndexItem = fakeQuery.SingleItem(filePathRaw)?.FileIndexItem;
			
			Assert.AreEqual(ColorClassParser.Color.Extras, fileIndexItem?.ColorClass);
			
			Assert.AreEqual(1,fileIndexItem?.SidecarExtensionsList.Count);
			Assert.AreEqual("xmp",fileIndexItem?.SidecarExtensionsList.ToList()[0]);
			
			var fileIndexItem2 = fakeQuery.SingleItem(filePathXmp)?.FileIndexItem;
			Assert.IsNotNull(fileIndexItem2);
		}
		
		[TestMethod]
		public async Task SingleItem_ShouldAddToSidecarFieldWhenSidecarIsAdded3()
		{
			const string filePathRaw = "/singleItem_ShouldAddToSidecarFieldWhenSidecarIsAdded3.dng";
			const string filePathXmp = "/singleItem_ShouldAddToSidecarFieldWhenSidecarIsAdded3.xmp";
			var lastEdited = DateTime.Now;

			// It should update the Sidecar field when a sidecar file is add to the directory
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{filePathRaw, filePathXmp}, new List<byte[]>{
					CreateAnImageNoExif.Bytes,
					CreateAnXmp.Bytes}, new List<DateTime>{lastEdited, lastEdited});
			
			var (fileHash, _) = await new FileHash(storage).GetHashCodeAsync(filePathRaw);

			var item = new FileIndexItem(filePathRaw)
			{
				FileHash = fileHash, // < right file hash
				Size = _iStorageFake.Info(filePathRaw).Size, // < right byte size
				LastEdited = lastEdited,
				ColorClass = ColorClassParser.Color.None
			};
			var item2 = new FileIndexItem(filePathXmp)
			{
				FileHash = "something_different", 
				Size = _iStorageFake.Info(filePathXmp).Size, 
				LastEdited = DateTime.MinValue // <-- different last edited
			};
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item,item2});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				storage, null!, new FakeIWebLogger());
			
			await sync.SingleFile( filePathXmp);
			
			var fileIndexItem = fakeQuery.SingleItem(filePathRaw)?.FileIndexItem;
			
			Assert.AreEqual(ColorClassParser.Color.Extras, fileIndexItem?.ColorClass);
			
			Assert.AreEqual(1,fileIndexItem?.SidecarExtensionsList.Count);
			Assert.AreEqual("xmp",fileIndexItem?.SidecarExtensionsList.ToList()[0]);
			
			var fileIndexItem2 = fakeQuery.SingleItem(filePathXmp)?.FileIndexItem;
			Assert.IsNotNull(fileIndexItem2);
		}
		
		[TestMethod]
		public async Task SingleItem_DbItem_Updated_TriggerDelegate()
		{
			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			var isCalled = false;
			Task TestTask(List<FileIndexItem> _)
			{
				isCalled = true;
				return Task.CompletedTask;
			}

			await sync.SingleFile("/test.jpg",
				new List<FileIndexItem> { item }, TestTask);
			 // % % % % Enter item here % % % % % 
			Assert.IsTrue(isCalled);
		}
		
		[TestMethod]
		public async Task SingleItem_DbItem_Updated_StatusDeleted()
		{
			var item = new FileIndexItem("/status_deleted.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			// % % % % Enter item here % % % % % 
			var result = (await sync.SingleFile("/status_deleted.jpg",new List<FileIndexItem>{item})).FirstOrDefault();  

			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted,result.Status);
		}
		
		[TestMethod]
		public async Task SingleItem_DbItem_NoContent_NoItemInDb()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result= (await sync.SingleFile("/test.jpg")).FirstOrDefault();  // % % % % Null value here % % % % % 
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result.Status);
			
			var count= (await fakeQuery.GetAllFilesAsync("/")).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg",fileIndexItem?.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem?.FileHash);
		}
		
		[TestMethod]
		public async Task SingleFile_DbItem_FileAlreadyExist_With_Same_ByteSize()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash,
				Size = _iStorageFake.Info("/test.jpg").Size, // < right byte size
				Tags = "the tags should not be updated" // <= the tags in /test.jpg is nothing
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item});
			
			var sync = new SyncSingleFile(new AppSettings {Verbose = true}, fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			await sync.SingleFile("/test.jpg",new List<FileIndexItem>{item}); // % % % % Enter item here % % % % % 
			
			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
		}

		[TestMethod]
		public async Task SingleFile_ShouldAddToSidecarFieldWhenSidecarIsAdded()
		{
			// It should update the Sidecar field when a sidecar file is add to the directory
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.dng", "/test.xmp"}, 
				new List<byte[]>{
					CreateAnImageNoExif.Bytes,
					CreateAnXmp.Bytes
				});
			
			var (fileHash, _) = await new FileHash(storage).GetHashCodeAsync("/test.dng");

			var item = new FileIndexItem("/test.dng")
			{
				FileHash = fileHash, // < right file hash
				Size = _iStorageFake.Info("/test.dng").Size, // < right byte size
			};
			var item2 = new FileIndexItem("/test.xmp")
			{
				FileHash = "some-thing", // < right file hash
				Size = 0, // < right byte size
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item, item2});
			
			var sync = new SyncSingleFile(new AppSettings {Verbose = true}, fakeQuery,
				storage, null, new FakeIWebLogger());
			
			//  Sync item here
			await sync.SingleFile("/test.xmp",
				new List<FileIndexItem> { item, item2 });
			
			var fileIndexItem = fakeQuery.SingleItem("/test.dng")?.FileIndexItem;
			
			Assert.AreEqual(1,fileIndexItem?.SidecarExtensionsList.Count);
			Assert.AreEqual("xmp",fileIndexItem?.SidecarExtensionsList.ToList()[0]);
		}
		
		[TestMethod]
		public async Task SingleFile_ShouldIgnoreSidecarFieldWhenItAlreadyExist()
		{
			// It should ignore the Sidecar field when a sidecar file when it already is there
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.dng", "/test.xmp"}, new List<byte[]>{
					CreateAnImageNoExif.Bytes,
					Array.Empty<byte>()});
			
			var (fileHash, _) = await new FileHash(storage).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash, // < right file hash
				Size = _iStorageFake.Info("/test.jpg").Size, // < right byte size
				SidecarExtensions = "xmp" // <- is already here
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> {item});
			
			var sync = new SyncSingleFile(new AppSettings {Verbose = true}, fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			await sync.SingleFile("/test.xmp",new List<FileIndexItem>{item});
			
			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;
			
			Assert.AreEqual(1,fileIndexItem?.SidecarExtensionsList.Count);
			Assert.AreEqual("xmp",fileIndexItem?.SidecarExtensionsList.ToList()[0]);
		}
		
		[TestMethod]
		public async Task FileAlreadyExist_With_Changed_FileHash_MetaDataCheck()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake)
				.GetHashCodeAsync("/color_class_test.jpg");
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/color_class_test.jpg")
				{
					FileHash = "THIS_IS_THE_OLD_HASH"
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			
			await sync.SingleFile("/color_class_test.jpg");
			
			var fileIndexItem = fakeQuery.SingleItem("/color_class_test.jpg")?.FileIndexItem;
			
			Assert.IsNotNull(fileIndexItem);
			Assert.AreEqual(fileHash, fileIndexItem.FileHash);
			Assert.AreEqual("Magland", fileIndexItem.LocationCity);
			Assert.AreEqual(9, fileIndexItem.Aperture);
			Assert.AreEqual(400, fileIndexItem.IsoSpeed);
			Assert.AreEqual("tete de balacha, bergtop, mist, flaine", fileIndexItem.Tags);
			Assert.AreEqual(ColorClassParser.Color.Winner, fileIndexItem.ColorClass);
		}
		
		[TestMethod]
		public async Task UpdateSidecarFileTest_True()
		{
			var sync = new SyncSingleFile(new AppSettings(), new FakeIQuery(new List<FileIndexItem>()),
				new FakeIStorage(new List<string>{"/"}, 
					new List<string>{"/test.jpg"}, 
					new List<byte[]> { CreateAnImageNoExif.Bytes }),null, new FakeIWebLogger());
			var result =
				await sync.UpdateSidecarFile("test.xmp",
					new List<FileIndexItem>());
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task UpdateSidecarFileTest_False()
		{
			var sync = new SyncSingleFile(new AppSettings(), new FakeIQuery(new List<FileIndexItem>()),
				new FakeIStorage(new List<string>{"/"}, 
					new List<string>{"/test.jpg"}, 
					new List<byte[]> { CreateAnImageNoExif.Bytes }),null, new FakeIWebLogger());
			var result =
				await sync.UpdateSidecarFile("test.jpg",
					new List<FileIndexItem>());
			Assert.IsFalse(result);
		}

	}
}
