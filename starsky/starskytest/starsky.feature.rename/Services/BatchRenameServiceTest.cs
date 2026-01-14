using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.rename.Models;
using starsky.feature.rename.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.rename.Services;

[TestClass]
public class BatchRenameServiceTest
{
	private readonly Query _query;
	private FileIndexItem _fileInExist = new();
	private FileIndexItem _fileInRoot = new();
	private FileIndexItem _folder1Exist = new();

	private FileIndexItem _folderExist = new();
	private FileIndexItem _parentFolder = new();

	public BatchRenameServiceTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();

		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(BatchRenameServiceTest));
		var options = builder.Options;
		var context = new ApplicationDbContext(options);

		var newImage = new CreateAnImage();

		var appSettings = new AppSettings
		{
			StorageFolder = PathHelper.AddBackslash(newImage.BasePath),
			ThumbnailTempFolder = newImage.BasePath
		};
		_query = new Query(context, appSettings, null,
			new FakeIWebLogger(), memoryCache);

		if ( _query.GetAllFilesAsync("/").Result.TrueForAll(p => p.FileName != newImage.FileName) )
		{
			context.FileIndex.Add(new FileIndexItem
			{
				FileName = newImage.FileName,
				ParentDirectory = "/",
				AddToDatabase = DateTime.UtcNow
			});
			context.SaveChanges();
		}
	}

	private async Task CreateFoldersAndFilesInDatabase()
	{
		_folderExist = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "exist",
			ParentDirectory = "/",
			AddToDatabase = DateTime.UtcNow,
			FileHash = "34567898765434567487984785487",
			IsDirectory = true
		});

		_fileInExist = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "file.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			AddToDatabase = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc)
		});

		_fileInRoot = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "root-file.jpg",
			ParentDirectory = "/",
			IsDirectory = false,
			AddToDatabase = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc)
		});

		_folder1Exist = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "folder1",
			ParentDirectory = "/",
			IsDirectory = true,
			FileHash = "3497867df894587"
		});

		_parentFolder = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "/", ParentDirectory = "/", IsDirectory = true
		});
	}

	[TestMethod]
	public async Task PreviewBatchRename_FileInExistsFolder_ReturnsExpectedMappings_ForValidFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_folderExist.FilePath!],
			[_fileInExist.FilePath!]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInExist.FilePath! };
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";
		var result = service.PreviewBatchRename(filePaths,
			tokenPattern);
		CollectionAssert.AreEqual(new List<string> { _fileInExist.FilePath! },
			result.Select(x => x.SourceFilePath).ToList());
		Assert.IsFalse(result[0].HasError);
		Assert.EndsWith(".jpg", result[0].TargetFilePath);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task PreviewBatchRename_FileInRoot_ReturnsExpectedMappings_ForValidFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([],
			[_fileInRoot.FilePath!]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInRoot.FilePath! };
		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}_{filenamebase}.{ext}";
		var result = service.PreviewBatchRename(filePaths,
			tokenPattern);
		CollectionAssert.AreEqual(new List<string> { _fileInRoot.FilePath! },
			result.Select(x => x.SourceFilePath).ToList());
		Assert.IsFalse(result[0].HasError);
		Assert.EndsWith(".jpg", result[0].TargetFilePath);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task PreviewBatchRename_FolderInRoot_ReturnsError()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_folderExist.FilePath!],
			[]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());
		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}_{filenamebase}.{ext}";
		var result = service.PreviewBatchRename([_folderExist.FilePath!],
			tokenPattern);
		CollectionAssert.AreEqual(new List<string> { _folderExist.FilePath! },
			result.Select(x => x.SourceFilePath).ToList());
		Assert.IsTrue(result[0].HasError);
		Assert.AreEqual("Is a directory", result[0].ErrorMessage);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task
		PreviewBatchRename_FileInRoot_ParentNull_ReturnsExpectedMappings_ForValidFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		_fileInRoot.ParentDirectory = null;

		var iStorage = new FakeIStorage([],
			[_fileInRoot.FilePath!]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInRoot.FilePath! };
		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}_{filenamebase}.{ext}";
		var result = service.PreviewBatchRename(filePaths,
			tokenPattern);
		CollectionAssert.AreEqual(new List<string> { _fileInRoot.FilePath! },
			result.Select(x => x.SourceFilePath).ToList());
		Assert.IsFalse(result[0].HasError);
		Assert.EndsWith(".jpg", result[0].TargetFilePath);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task PreviewBatchRenameAsync_ReturnsError_ForInvalidPattern()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_folderExist.FilePath!],
			[_fileInExist.FilePath!]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInExist.FilePath! };
		const string tokenPattern = "{invalidtoken}";
		var result = service.PreviewBatchRename(filePaths, tokenPattern);
		CollectionAssert.AllItemsAreNotNull(result);
		Assert.IsTrue(result.Any(x => x.HasError));
		Assert.Contains("Invalid pattern", result[0].ErrorMessage!);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public void PreviewBatchRenameAsync_ReturnsError_WhenFileNotFound()
	{
		var iStorage =
			new FakeIStorage([_folderExist.FilePath!], new List<string>());
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { "/notfound.jpg" };
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";
		var result = service.PreviewBatchRename(filePaths, tokenPattern);
		CollectionAssert.AllItemsAreNotNull(result);
		Assert.IsTrue(result.Any(x => x.HasError));
		Assert.AreEqual("File not found in database", result[0].ErrorMessage);
	}

	[TestMethod]
	public void PreviewBatchRenameAsync_ReturnsEmptyList_WhenNoFiles()
	{
		var iStorage = new FakeIStorage([], []);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string>();
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";
		var result = service.PreviewBatchRename(filePaths, tokenPattern);
		CollectionAssert.AllItemsAreNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_IsEmpty()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new BatchRenameService(new FakeIQuery([new FileIndexItem("/test.jpg")]),
				iStorage, new FakeIWebLogger());
		var result = await service.ExecuteBatchRenameAsync([]);

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_NotFound()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new BatchRenameService(new FakeIQuery([new FileIndexItem("/test.jpg")]),
				iStorage, new FakeIWebLogger());
		var result = await service.ExecuteBatchRenameAsync([
			new BatchRenameMapping
			{
				SourceFilePath = "/notfound.jpg",
				TargetFilePath = "/newname.jpg",
				HasError = false,
				RelatedFilePaths = []
			}
		]);

		Assert.HasCount(1, result);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Null()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new BatchRenameService(new FakeIQueryException(new AccessViolationException("test")),
				iStorage, new FakeIWebLogger());
		var result = await service.ExecuteBatchRenameAsync([
			new BatchRenameMapping
			{
				SourceFilePath = null!,
				TargetFilePath = null!,
				HasError = false,
				RelatedFilePaths = []
			}
		]);

		Assert.HasCount(1, result);
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result[0].Status);
	}

	[TestMethod]
	public void PreviewBatchRename_Null()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new BatchRenameService(new FakeIQueryException(new AccessViolationException("test")),
				iStorage, new FakeIWebLogger());
		var result = service.PreviewBatchRename(
			[null!], "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}");

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void PreviewBatchRename_InvalidFileName()
	{
		var iStorage = new FakeIStorage([], ["/test.jpg"]);
		var service =
			new BatchRenameService(new FakeIQuery([new FileIndexItem("/test.jpg")]),
				iStorage, new FakeIWebLogger());
		var result = service.PreviewBatchRename(
			["/test.jpg"], "{yyyy}{MM}{dd}_{filenamebase}{seqn}__{ext}");

		Assert.HasCount(1, result);
		Assert.IsTrue(result[0].HasError);
		Assert.AreEqual(
			"Failed to generate filename: Generated filename is invalid: 00010101_test__jpg",
			result[0].ErrorMessage);
	}


	[TestMethod]
	public async Task ExecuteBatchRenameAsync_BatchRename_SimpleFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_parentFolder.FilePath!, _folderExist.FilePath!],
			[_fileInExist.FilePath!, _fileInRoot.FilePath!]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());

		var mappings = new List<BatchRenameMapping>
		{
			new()
			{
				SourceFilePath = _fileInExist.FilePath!,
				TargetFilePath = "/exist/20220506_000000.jpg",
				HasError = false,
				RelatedFilePaths = []
			},
			new()
			{
				SourceFilePath = _fileInRoot.FilePath!,
				TargetFilePath = "/20220506_000000.jpg",
				HasError = false,
				RelatedFilePaths = []
			}
		};

		var result = await service.ExecuteBatchRenameAsync(mappings, false);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).ToList();

		Assert.HasCount(2, filteredResults);
		Assert.AreEqual("/exist/20220506_000000.jpg", filteredResults[0].FilePath);
		Assert.AreEqual("/20220506_000000.jpg", filteredResults[1].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[1].Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_WithPreview_SimpleFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_parentFolder.FilePath!, _folderExist.FilePath!],
			[_fileInExist.FilePath!, _fileInRoot.FilePath!]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}-{MM}-{dd}_{HH}-{mm}-{ss}_a.{ext}";
		var mappings = service.PreviewBatchRename(
			[_fileInExist.FilePath!, _fileInRoot.FilePath!],
			tokenPattern);

		var result = await service.ExecuteBatchRenameAsync(mappings, false);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).ToList();

		Assert.HasCount(2, filteredResults);
		Assert.AreEqual("/exist/2022-05-06_00-00-00_a.jpg", filteredResults[0].FilePath);
		Assert.AreEqual("/2022-05-06_00-00-00_a-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[1].Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Sequence_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();
		// Simulate two files with the same datetime, requiring sequence handling
		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});
		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});
		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!
		]);
		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());

		var mappings = new List<BatchRenameMapping>
		{
			new()
			{
				SourceFilePath = file1.FilePath!,
				TargetFilePath = "/exist/20260101_180000.jpg",
				HasError = false,
				RelatedFilePaths = []
			},
			new()
			{
				SourceFilePath = file2.FilePath!,
				TargetFilePath = "/exist/20260101_180000-1.jpg",
				HasError = false,
				RelatedFilePaths = []
			}
		};

		var result = await service.ExecuteBatchRenameAsync(mappings, false);
		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).ToList();

		Assert.HasCount(2, filteredResults);
		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[0].FilePath);
		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[1].Status);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Sequence_Raw_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();

		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			FileHash = "DSC0001.jpg",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.arw",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.jpg",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.arw",
			AddToDatabase = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc)
		});

		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!,
			new FileIndexItem(file1Raw.FilePath!).FilePath!,
			new FileIndexItem(file2Raw.FilePath!).FilePath!
		]);

		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}.{ext}";

		var mappings = service.PreviewBatchRename(
			[file1.FilePath!, file2.FilePath!, file2Raw.FilePath!, file1Raw.FilePath!],
			tokenPattern);

		var result = await service.ExecuteBatchRenameAsync(mappings);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).OrderBy(p => p.FileName).ToList();

		Assert.HasCount(4, filteredResults);

		Assert.AreEqual("/exist/20260101_180000-1.arw", filteredResults[0].FilePath);
		Assert.AreEqual("DSC0002.arw", filteredResults[0].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual("DSC0002.jpg", filteredResults[1].FileHash);

		Assert.AreEqual("/exist/20260101_180000.arw", filteredResults[2].FilePath);
		Assert.AreEqual("DSC0001.arw", filteredResults[2].FileHash);

		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[3].FilePath);
		Assert.AreEqual("DSC0001.jpg", filteredResults[3].FileHash);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await _query.RemoveItemAsync(file1Raw);
		await _query.RemoveItemAsync(file2Raw);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Sequence_RawXmp_Explicit_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();

		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			FileHash = "DSC0001.jpg",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.arw",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.jpg",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.arw",
			AddToDatabase = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc)
		});

		var file2Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!,
			new FileIndexItem(file1Raw.FilePath!).FilePath!,
			new FileIndexItem(file2Raw.FilePath!).FilePath!,
			new FileIndexItem(file1Xmp.FilePath!).FilePath!,
			new FileIndexItem(file2Xmp.FilePath!).FilePath!
		]);

		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}.{ext}";

		var mappings = service.PreviewBatchRename(
			[
				file1.FilePath!, file2.FilePath!,
				file2Raw.FilePath!, file1Raw.FilePath!,
				file1Xmp.FilePath!, file2Xmp.FilePath!
			],
			tokenPattern);

		var result = await service.ExecuteBatchRenameAsync(mappings);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).OrderBy(p => p.FileName).ToList();

		Assert.HasCount(6, filteredResults);

		Assert.AreEqual("/exist/20260101_180000-1.arw", filteredResults[0].FilePath);
		Assert.AreEqual("DSC0002.arw", filteredResults[0].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual("DSC0002.jpg", filteredResults[1].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.xmp", filteredResults[2].FilePath);
		Assert.AreEqual("DSC0002.xmp", filteredResults[2].FileHash);

		Assert.AreEqual("/exist/20260101_180000.arw", filteredResults[3].FilePath);
		Assert.AreEqual("DSC0001.arw", filteredResults[3].FileHash);

		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[4].FilePath);
		Assert.AreEqual("DSC0001.jpg", filteredResults[4].FileHash);

		Assert.AreEqual("/exist/20260101_180000.xmp", filteredResults[5].FilePath);
		Assert.AreEqual("DSC0001.xmp", filteredResults[5].FileHash);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await _query.RemoveItemAsync(file1Raw);
		await _query.RemoveItemAsync(file2Raw);
		await _query.RemoveItemAsync(file1Xmp);
		await _query.RemoveItemAsync(file2Xmp);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Sequence_RawXmp_Implicit_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();

		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			FileHash = "DSC0001.jpg",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.arw",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.jpg",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.arw",
			AddToDatabase = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc)
		});

		var file2Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!,
			new FileIndexItem(file1Raw.FilePath!).FilePath!,
			new FileIndexItem(file2Raw.FilePath!).FilePath!,
			new FileIndexItem(file1Xmp.FilePath!).FilePath!,
			new FileIndexItem(file2Xmp.FilePath!).FilePath!
		]);

		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}.{ext}";

		var mappings = service.PreviewBatchRename(
			[
				file1.FilePath!, file2.FilePath!
				// so related files are found implicitly
			],
			tokenPattern);

		var result = await service.ExecuteBatchRenameAsync(mappings);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).OrderBy(p => p.FileName).ToList();

		Assert.HasCount(6, filteredResults);

		Assert.AreEqual("/exist/20260101_180000-1.arw", filteredResults[0].FilePath);
		Assert.AreEqual("DSC0002.arw", filteredResults[0].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual("DSC0002.jpg", filteredResults[1].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.xmp", filteredResults[2].FilePath);
		Assert.AreEqual("DSC0002.xmp", filteredResults[2].FileHash);

		Assert.AreEqual("/exist/20260101_180000.arw", filteredResults[3].FilePath);
		Assert.AreEqual("DSC0001.arw", filteredResults[3].FileHash);

		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[4].FilePath);
		Assert.AreEqual("DSC0001.jpg", filteredResults[4].FileHash);

		Assert.AreEqual("/exist/20260101_180000.xmp", filteredResults[5].FilePath);
		Assert.AreEqual("DSC0001.xmp", filteredResults[5].FileHash);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await _query.RemoveItemAsync(file1Raw);
		await _query.RemoveItemAsync(file2Raw);
		await _query.RemoveItemAsync(file1Xmp);
		await _query.RemoveItemAsync(file2Xmp);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task
		ExecuteBatchRenameAsync_Sequence_RawXmp_Implicit_CollectionsFalse_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();

		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			FileHash = "DSC0001.jpg",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.arw",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.jpg",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.arw",
			AddToDatabase = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc)
		});

		var file2Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!,
			new FileIndexItem(file1Raw.FilePath!).FilePath!,
			new FileIndexItem(file2Raw.FilePath!).FilePath!,
			new FileIndexItem(file1Xmp.FilePath!).FilePath!,
			new FileIndexItem(file2Xmp.FilePath!).FilePath!
		]);

		var service = new BatchRenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}.{ext}";

		var mappings = service.PreviewBatchRename(
			[
				file1.FilePath!, file2.FilePath!
				// so related files are found implicitly
			],
			tokenPattern, false); // disable collections

		var result = await service.ExecuteBatchRenameAsync(mappings);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).OrderBy(p => p.FileName).ToList();

		Assert.HasCount(2, filteredResults);
		
		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[0].FilePath);
		Assert.AreEqual("DSC0002.jpg", filteredResults[0].FileHash);

		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[1].FilePath);
		Assert.AreEqual("DSC0001.jpg", filteredResults[1].FileHash);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await _query.RemoveItemAsync(file1Raw);
		await _query.RemoveItemAsync(file2Raw);
		await _query.RemoveItemAsync(file1Xmp);
		await _query.RemoveItemAsync(file2Xmp);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public void PreviewBatchRename_WithJsonSidecarFile_IncludesSidecarInMapping()
	{
		// Arrange
		const string sourceFilePath = "/folder/testfile.jpg";
		var sidecarFilePath = JsonSidecarLocation.JsonLocation(sourceFilePath);
		var filePaths = new List<string> { sourceFilePath, sidecarFilePath };
		var iStorage = new FakeIStorage(["/folder"], [sourceFilePath, sidecarFilePath]);
		var fakeQuery = new FakeIQuery([
			new FileIndexItem(sourceFilePath)
			{
				FileName = "testfile.jpg",
				ParentDirectory = "/folder",
				DateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc)
			},
			new FileIndexItem(sidecarFilePath)
			{
				FileName = ".starsky.testfile.jpg.json",
				ParentDirectory = "/folder",
				DateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc)
			}
		]);
		var service = new BatchRenameService(fakeQuery, iStorage, new FakeIWebLogger());
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";

		// Act
		var result = service.PreviewBatchRename(filePaths, tokenPattern);

		// Assert
		Assert.HasCount(2, result);
		Assert.IsTrue(result.Any(x => x.SourceFilePath == sourceFilePath));
		Assert.IsTrue(result.Any(x => x.SourceFilePath == sidecarFilePath));
		var mainFile = result.First(x => x.SourceFilePath == sourceFilePath);
		var sidecar = result.First(x => x.SourceFilePath == sidecarFilePath);
		Assert.IsFalse(mainFile.HasError);
		Assert.IsFalse(sidecar.HasError);
		Assert.EndsWith(".jpg", mainFile.TargetFilePath);
		Assert.EndsWith(".json", sidecar.TargetFilePath);
		Assert.Contains(".starsky.", sidecar.TargetFilePath);
	}

	private async Task RemoveFoldersAndFilesInDatabase()
	{
		Assert.IsNotNull(_folderExist.FilePath);
		Assert.IsNotNull(_folder1Exist.FilePath);
		Assert.IsNotNull(_fileInExist.FilePath);
		Assert.IsNotNull(_parentFolder.FilePath);
		Assert.IsNotNull(_fileInRoot.FilePath);

		await _query.RemoveItemAsync(_folderExist);
		await _query.RemoveItemAsync(_folder1Exist);
		await _query.RemoveItemAsync(_fileInExist);
		await _query.RemoveItemAsync(_parentFolder);
		await _query.RemoveItemAsync(_fileInRoot);
	}
}
