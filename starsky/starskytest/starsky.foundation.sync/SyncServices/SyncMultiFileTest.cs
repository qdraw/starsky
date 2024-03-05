using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public sealed class SyncMultiFileTest
	{
		private readonly FakeIStorage _iStorageFake;
		private readonly DateTime _lastEditedDateTime;

		public SyncMultiFileTest()
		{
			_lastEditedDateTime = new DateTime(2020, 02, 02,
				01, 01, 01, kind: DateTimeKind.Local);
			_iStorageFake = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.jpg", "/color_class_test.jpg", "/status_deleted.jpg" },
				new List<byte[]>
				{
					CreateAnImageNoExif.Bytes.ToArray(),
					CreateAnImageColorClass.Bytes.ToArray(),
					CreateAnImageStatusDeleted.Bytes.ToArray()
				},
				new List<DateTime>
				{
					_lastEditedDateTime, _lastEditedDateTime, _lastEditedDateTime
				});
		}

		[TestMethod]
		public async Task FileType_NotSupported()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result = await sync.MultiFile(new List<string> { "/non_exist.ext" });

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result[0].Status);
		}

		[TestMethod]
		public async Task MultiFile_ImageFormat_Corrupt()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());

			var storage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/corrupt.jpg" },
				new List<byte[]> { new byte[5] });

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				storage, null, new FakeIWebLogger());
			var result = await sync.MultiFile(new List<string> { "/corrupt.jpg" });

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result[0].Status);
		}

		[TestMethod]
		public async Task MultiFile_AddNewFile()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			var result = await sync.MultiFile(new List<string> { "/test.jpg" });
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);

			// should add files to db
			var detailView = fakeQuery.SingleItem("/test.jpg");
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg", fileIndexItem?.FilePath);

			// should not duplicate add items
			var count =
				( await fakeQuery.GetAllFilesAsync("/") ).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1, count);
		}

		[TestMethod]
		public async Task MultiFile_AddNewFile_StatusDeleted()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			var result = await sync.MultiFile(new List<string> { "/status_deleted.jpg" });

			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, result[0].Status);
		}


		[TestMethod]
		public async Task MultiFile_AddNewFile_WithParentFolders()
		{
			var iStorageFake = new FakeIStorage(new List<string> { "/", "/level", "/level/deep" },
				new List<string> { "/level/deep/test.jpg" },
				new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				iStorageFake, null, new FakeIWebLogger());
			await sync.MultiFile(new List<string> { "/level/deep/test.jpg" });

			var detailView = fakeQuery.SingleItem("/level/deep/test.jpg");
			// should add files to db
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/level/deep/test.jpg", fileIndexItem?.FilePath);

			// should not duplicate add items
			var count = ( await fakeQuery.GetAllFilesAsync("/level/deep") )
				.Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1, count);

			// should add parent items
			// root folder
			var rootFolder = fakeQuery.SingleItem("/")?.FileIndexItem;
			Assert.IsNotNull(rootFolder);
			Assert.IsTrue(rootFolder.IsDirectory);

			// sub folder 'level folder
			var levelFolder = fakeQuery.SingleItem("/level")?.FileIndexItem;
			Assert.IsNotNull(levelFolder);
			Assert.IsTrue(levelFolder.IsDirectory);

			// sub folder 'level deep folder
			var levelDeepFolder = fakeQuery.SingleItem("/level/deep")?.FileIndexItem;
			Assert.IsNotNull(levelDeepFolder);
			Assert.IsTrue(levelDeepFolder.IsDirectory);
		}

		[TestMethod]
		public async Task MultiFile_FileAlreadyExist_WithSameFileHash()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash, LastEdited = _lastEditedDateTime
				}
			});

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result = await sync.MultiFile(new List<string> { "/test.jpg" });

			Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result[0].Status);

			var count =
				( await fakeQuery.GetAllFilesAsync("/") ).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1, count);

			var detailView = fakeQuery.SingleItem("/test.jpg");
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg", fileIndexItem?.FilePath);
		}

		[TestMethod]
		public async Task MultiFile_FileAlreadyExist_WithSameFileHash_ShouldNotTrigger()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/"),
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash, LastEdited = _lastEditedDateTime
				}
			});

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			var isCalled = false;

			Task TestTask(List<FileIndexItem> _)
			{
				isCalled = true;
				return Task.CompletedTask;
			}

			await sync.MultiFile(new List<string> { "/test.jpg" }, TestTask);

			Assert.IsFalse(isCalled);
		}


		[TestMethod]
		public async Task MultiFile_FileAlreadyExist_WithSameFileHash_ParentDirNotExistSoTrigger()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				// no parent folder in database
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash, LastEdited = _lastEditedDateTime
				}
			});

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			var isCalled = false;

			Task TestTask(List<FileIndexItem> _)
			{
				isCalled = true;
				return Task.CompletedTask;
			}

			var result = await sync.MultiFile(new List<string> { "/test.jpg" }, TestTask);

			Assert.IsTrue(isCalled);

			var items = result.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList();
			Assert.AreEqual(1, items.Count);
			Assert.AreEqual("/", items[0].FilePath);
		}

		[TestMethod]
		public async Task MultiFile_FileAlreadyExist_With_Same_LastEditedTime()
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

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			var result = await sync.MultiFile(new List<string> { "/test.jpg" });

			Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result[0].Status);

			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
		}

		[TestMethod]
		public async Task MultiFile_FileAlreadyExist_With_Changed_FileHash()
		{
			const string currentFilePath = "/test_date.jpg";
			_iStorageFake.FileCopy("/test.jpg", currentFilePath);
			_iStorageFake.SetLastWriteTime(currentFilePath, DateTime.UtcNow);

			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync(currentFilePath);

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem(currentFilePath)
				{
					FileHash = "THIS_IS_THE_OLD_HASH",
					Size = 99999999, // % % % that's not the right size % % %
				}
			});

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result = await sync.MultiFile(new List<string> { currentFilePath });

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);

			var count =
				( await fakeQuery.GetAllFilesAsync("/") ).Count(p => p.FilePath == currentFilePath);
			Assert.AreEqual(1, count);

			var detailView = fakeQuery.SingleItem(currentFilePath);

			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual(currentFilePath, fileIndexItem?.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem?.FileHash);

			// Should be around now-ish
			Assert.AreEqual(DateTime.UtcNow.Day, fileIndexItem?.LastEdited.Day);
			Assert.AreEqual(DateTime.UtcNow.Month, fileIndexItem?.LastEdited.Month);
			Assert.AreEqual(DateTime.UtcNow.Hour, fileIndexItem?.LastEdited.Hour);
			Assert.AreEqual(DateTime.UtcNow.Minute, fileIndexItem?.LastEdited.Minute);
		}

		[TestMethod]
		public async Task MultiFile_FileAlreadyExist_With_Changed_FileHash_ShouldTriggerDelegate()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
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

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			await sync.MultiFile(new List<string> { "/test.jpg" }, TestTask);

			Assert.IsTrue(isCalled);
		}

		[TestMethod]
		public async Task MultiFile_DbItem_Updated()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item });

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			var result =
				await sync.MultiFile(new List<FileIndexItem>
				{
					item
				}); // % % % % Enter item here % % % % % 

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);

			var count =
				( await fakeQuery.GetAllFilesAsync("/") ).Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1, count);

			var detailView = fakeQuery.SingleItem("/test.jpg");

			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg", fileIndexItem?.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem?.FileHash);
		}

		[TestMethod]
		public async Task MultiFile_DbItem_Updated_TriggerDelegate()
		{
			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item });

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			var isCalled = false;

			Task TestTask(List<FileIndexItem> _)
			{
				isCalled = true;
				return Task.CompletedTask;
			}

			await sync.MultiFile(new List<FileIndexItem> { item },
				TestTask); // % % % % Enter item here % % % % % 
			Assert.IsTrue(isCalled);
		}

		[TestMethod]
		public async Task MultiFile_DbItem_Updated_StatusDeleted()
		{
			var item = new FileIndexItem("/status_deleted.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item });

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result =
				await sync.MultiFile(new List<FileIndexItem>
				{
					item
				}); // % % % % Enter item here % % % % % 

			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, result[0].Status);
		}

		[TestMethod]
		public async Task MultiFile_DbItem_NoContent_NoItemInDb()
		{
			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = "THIS_IS_THE_OLD_HASH",
				Size = 99999999 // % % % that's not the right size % % %
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item });

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			var result =
				await sync.MultiFile(
					null as List<FileIndexItem>); // % % % % Null value here % % % % % 

			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public async Task MultiFile_DbItem_FileAlreadyExist_With_Same_ByteSize()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash,
				Size = _iStorageFake.Info("/test.jpg").Size, // < right byte size
				Tags = "the tags should not be updated" // <= the tags in /test.jpg is nothing
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item });

			var sync = new SyncMultiFile(new AppSettings { Verbose = true }, fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			await sync.MultiFile(new List<FileIndexItem>
			{
				item
			}); // % % % % Enter item here % % % % % 

			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem?.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem?.Tags);
		}

		[TestMethod]
		public async Task MultiFile_ShouldAddToSidecarFieldWhenSidecarIsAdded()
		{
			var lastEdited = DateTime.Now;
			// It should update the Sidecar field when a sidecar file is add to the directory
			var storage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.dng", "/test.xmp" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() },
				new List<DateTime> { lastEdited, lastEdited });

			var (fileHash, _) = await new FileHash(storage).GetHashCodeAsync("/test.dng");

			var item = new FileIndexItem("/test.dng")
			{
				FileHash = fileHash, // < right file hash
				Size = _iStorageFake.Info("/test.dng").Size, // < right byte size
				LastEdited = lastEdited
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item });

			var sync = new SyncMultiFile(new AppSettings { Verbose = true }, fakeQuery,
				storage, null, new FakeIWebLogger());
			await sync.MultiFile(new List<string> { "/test.dng", "/test.xmp" });

			var fileIndexItem = fakeQuery.SingleItem("/test.dng")?.FileIndexItem;

			Assert.AreEqual(1, fileIndexItem?.SidecarExtensionsList.Count);
			Assert.AreEqual("xmp", fileIndexItem?.SidecarExtensionsList.ToList()[0]);

			var fileIndexItem2 = fakeQuery.SingleItem("/test.xmp")?.FileIndexItem;
			Assert.IsNotNull(fileIndexItem2);
		}

		[TestMethod]
		public async Task MultiFile_ShouldAddToSidecarFieldWhenSidecarIsAdded_Equal()
		{
			var lastEdited = DateTime.Now;

			// It should update the Sidecar field when a sidecar file is add to the directory
			var storage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.dng", "/test.xmp" },
				new List<byte[]>
				{
					CreateAnImageNoExif.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray()
				});

			var (fileHash, _) = await new FileHash(storage).GetHashCodeAsync("/test.dng");

			var item = new FileIndexItem("/test.dng")
			{
				FileHash = fileHash, // < right file hash
				Size = _iStorageFake.Info("/test.dng").Size, // < right byte size
				LastEdited = lastEdited
			};
			var item2 = new FileIndexItem("/test.xmp")
			{
				FileHash = "something_different",
				Size = _iStorageFake.Info("/test.xmp").Size,
				LastEdited = lastEdited
			};

			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item, item2 });

			var sync = new SyncMultiFile(new AppSettings { Verbose = true }, fakeQuery,
				storage, null, new FakeIWebLogger());
			await sync.MultiFile(new List<string> { "/test.dng", "/test.xmp" });

			var fileIndexItem = fakeQuery.SingleItem("/test.dng")?.FileIndexItem;

			Assert.AreEqual(ColorClassParser.Color.DoNotChange, fileIndexItem?.ColorClass);

			Assert.AreEqual(1, fileIndexItem?.SidecarExtensionsList.Count);
			Assert.AreEqual("xmp", fileIndexItem?.SidecarExtensionsList.ToList()[0]);

			var fileIndexItem2 = fakeQuery.SingleItem("/test.xmp")?.FileIndexItem;
			Assert.IsNotNull(fileIndexItem2);
		}

		[TestMethod]
		public async Task MultiFile_ShouldIgnoreSidecarFieldWhenItAlreadyExist()
		{
			// It should ignore the Sidecar field when a sidecar file when it already is there
			var storage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.dng", "/test.xmp" },
				new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray(), Array.Empty<byte>() });

			var (fileHash, _) = await new FileHash(storage).GetHashCodeAsync("/test.jpg");

			var item = new FileIndexItem("/test.jpg")
			{
				FileHash = fileHash, // < right file hash
				Size = _iStorageFake.Info("/test.jpg").Size, // < right byte size
				SidecarExtensions = "xmp" // <- is already here
			};
			var fakeQuery = new FakeIQuery(new List<FileIndexItem> { item });

			var sync = new SyncMultiFile(new AppSettings { Verbose = true }, fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());
			await sync.MultiFile(new List<FileIndexItem> { item });

			var fileIndexItem = fakeQuery.SingleItem("/test.jpg")?.FileIndexItem;

			Assert.AreEqual(1, fileIndexItem?.SidecarExtensionsList.Count);
			Assert.AreEqual("xmp", fileIndexItem?.SidecarExtensionsList.ToList()[0]);
		}

		[TestMethod]
		public async Task MultiFile_FileAlreadyExist_With_Changed_FileHash_MetaDataCheck()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake)
				.GetHashCodeAsync("/color_class_test.jpg");

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/color_class_test.jpg") { FileHash = "THIS_IS_THE_OLD_HASH" }
			});

			var sync = new SyncMultiFile(new AppSettings(), fakeQuery,
				_iStorageFake, null, new FakeIWebLogger());

			await sync.MultiFile(new List<string> { "/color_class_test.jpg" });

			var fileIndexItem = fakeQuery.SingleItem("/color_class_test.jpg")?.FileIndexItem;

			Assert.IsNotNull(fileIndexItem);
			Assert.AreEqual(fileHash, fileIndexItem.FileHash);
			Assert.AreEqual("Magland", fileIndexItem.LocationCity);
			Assert.AreEqual(9, fileIndexItem.Aperture);
			Assert.AreEqual(400, fileIndexItem.IsoSpeed);
			Assert.AreEqual("tete de balacha, bergtop, mist, flaine", fileIndexItem.Tags);
			Assert.AreEqual(ColorClassParser.Color.Winner, fileIndexItem.ColorClass);
		}
	}
}
