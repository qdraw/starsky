using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryGetNextPrevInFolderTest
{
	private readonly ApplicationDbContext _context;
	private readonly Query _query;

	public QueryGetNextPrevInFolderTest()
	{
		// Initialize the context and query object
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(QueryGetNextPrevInFolderTest));
		_context = new ApplicationDbContext(builder.Options);
		_query = new Query(_context, new AppSettings(),
			new FakeIServiceScopeFactory(nameof(QueryGetNextPrevInFolderTest)),
			new FakeIWebLogger(), new FakeMemoryCache());
	}

	[TestMethod]
	// directory is included
	[DataRow("/collection", "/collection/20241106_155823_DSC00339", 4,
		"/collection/20241106_155758_DSC00338.jpg",
		"/collection/20241106_155823_DSC00339",
		"/collection/20241106_155823_DSC00339.jpg",
		"/collection/20241106_155825_DSC00340.jpg")]
	// raw is included
	[DataRow("/collection", "/collection/20241106_155758_DSC00338.arw", 4,
		"/collection/20241106_155758_DSC00338.arw",
		"/collection/20241106_155758_DSC00338.jpg",
		"/collection/20241106_155823_DSC00339.jpg",
		"/collection/20241106_155825_DSC00340.jpg")]
	// selected file is already favorite
	[DataRow("/collection", "/collection/20241106_155825_DSC00340.jpg", 3,
		"/collection/20241106_155758_DSC00338.jpg",
		"/collection/20241106_155823_DSC00339.jpg",
		"/collection/20241106_155825_DSC00340.jpg",
		null)]
	// not found should be skipped
	[DataRow("/collection", "/collection/NOT_FOUND_FILE.jpg", 3,
		"/collection/20241106_155758_DSC00338.jpg",
		"/collection/20241106_155823_DSC00339.jpg",
		"/collection/20241106_155825_DSC00340.jpg",
		null)]
	public async Task QueryGetNextPrevInFolder_ReturnsExpectedResult(string parentFolderPath,
		string currentFolder, int expectedCount, string expectedFilePath1, string expectedFilePath2,
		string expectedFilePath3, string? expectedFilePath4)
	{
		// Arrange
		var items = new List<FileIndexItem>
		{
			new()
			{
				FilePath = "/collection/20241106_155758_DSC00338.arw",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new()
			{
				FilePath = "/collection/20241106_155758_DSC00338.jpg",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/collection/20241106_155823_DSC00339",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.directory
			},
			new()
			{
				FilePath = "/collection/20241106_155823_DSC00339.arw",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new()
			{
				FilePath = "/collection/20241106_155823_DSC00339.jpg",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/collection/20241106_155825_DSC00340.arw",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new()
			{
				FilePath = "/collection/20241106_155825_DSC00340.jpg",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		_context.FileIndex.AddRange(items);
		await _context.SaveChangesAsync();

		// Act
		var result = _query.QueryGetNextPrevInFolder(parentFolderPath, currentFolder);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(expectedCount, result.Count);
		Assert.AreEqual(expectedFilePath1, result[0].FilePath);
		Assert.AreEqual(expectedFilePath2, result[1].FilePath);
		Assert.AreEqual(expectedFilePath3, result[2].FilePath);
		if ( expectedFilePath4 != null )
		{
			Assert.AreEqual(expectedFilePath4, result[3].FilePath);
		}

		await _query.RemoveItemAsync(items);
	}

	[TestMethod]
	public async Task QueryGetNextPrevInFolder_Duplicate()
	{
		const string parentFolderPath = "/collection";
		const string currentFolder = "/collection/DSC00338.jpg";
		var items = new List<FileIndexItem>
		{
			new()
			{
				FilePath = "/test/20241107_133153_DSC00647.jpg",
				FileHash = "IQJOWVXWYWKCJX3WI4QVBLLPGQ",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/test/20241107_133428_DSC00651_e.jpg",
				FileHash = "TKI2Z5WAP7GR3RKMKIUV3D5RZY",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/test/20241107_133428_DSC00651_e.jpg",
				FileHash = "TKI2Z5WAP7GR3RKMKIUV3D5RZY",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/test/20241107_133428_DSC00651.jpg",
				FileHash = "KXBZECUFUOIG3BPPU4HCSDEHZM",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		_context.FileIndex.AddRange(items);
		await _context.SaveChangesAsync();

		// Act
		var result = _query.QueryGetNextPrevInFolder(parentFolderPath, currentFolder);

		Assert.IsNotNull(result);
		Assert.AreEqual(items.Count - 1, result.Count);

		// clean
		await _query.RemoveItemAsync(items);
	}

	[TestMethod]
	public async Task QueryGetNextPrevInFolder_DoesNotIncludeMetaJsonOrXmp()
	{
		// Arrange
		const string parentFolderPath = "/collection";
		const string currentFolder = "/collection/DSC00338.jpg";
		var items = new List<FileIndexItem>
		{
			new()
			{
				FilePath = "/collection/DSC00338.jpg",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/collection/DSC00340.json",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.meta_json
			},
			new()
			{
				FilePath = "/collection/DSC00341.xmp",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
			}
		};

		_context.FileIndex.AddRange(items);
		await _context.SaveChangesAsync();

		// Act
		var result = _query.QueryGetNextPrevInFolder(parentFolderPath, currentFolder);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Exists(p =>
			p.ImageFormat == ExtensionRolesHelper.ImageFormat.meta_json));
		Assert.IsFalse(result.Exists(p => p.ImageFormat == ExtensionRolesHelper.ImageFormat.xmp));

		await _query.RemoveItemAsync(items);
	}

	[TestMethod]
	public void QueryGetNextPrevInFolder_HandlesMySqlProtocolException()
	{
		// Arrange
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		var customContext = new MySqlProtocolExceptionDbContext(builder.Options);
		var serviceScopeFactory = new FakeIServiceScopeFactory();
		var query = new Query(customContext, new AppSettings(), serviceScopeFactory,
			new FakeIWebLogger(), new FakeMemoryCache());

		const string parentFolderPath = "/collection";
		const string currentFolder = "/collection/20241106_155758_DSC00338";
		var items = new List<FileIndexItem>
		{
			new()
			{
				FilePath = "/collection/20241106_155758_DSC00338.arw",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			}
		};

		var serviceScope = serviceScopeFactory.CreateScope();
		var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		dbContext.FileIndex.AddRange(items);
		dbContext.SaveChanges();

		// Act
		var result = query.QueryGetNextPrevInFolder(parentFolderPath, currentFolder);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(items.Count, result.Count);
		Assert.AreEqual(items[0].FilePath, result[0].FilePath);

		// Cleanup
		dbContext.Remove(items[0]);
		dbContext.SaveChanges();
	}

	private sealed class MySqlProtocolExceptionDbContext(
		DbContextOptions<ApplicationDbContext> options)
		: ApplicationDbContext(options)
	{
		public override DbSet<FileIndexItem> FileIndex
		{
			get
			{
				var exceptionType = typeof(MySqlProtocolException);
				var constructor = exceptionType.GetConstructor(
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					[typeof(string)],
					null
				) ?? throw new InvalidOperationException("Constructor not found.");

				var exceptionInstance =
					( MySqlProtocolException ) constructor.Invoke(new object[]
					{
						"Test exception"
					});
				throw exceptionInstance;
			}
		}
	}
}
