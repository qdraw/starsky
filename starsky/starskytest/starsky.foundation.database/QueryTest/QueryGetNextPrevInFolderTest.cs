using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.Controllers;
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
		_query = new Query(_context, new AppSettings(), new FakeIServiceScopeFactory(),
			new FakeIWebLogger(), new FakeMemoryCache());
	}

	[TestMethod]
	public void QueryGetNextPrevInFolder_ReturnsExpectedResult()
	{
		// Arrange
		const string parentFolderPath = "/map_collection_bug";
		const string currentFolder = "/map_collection_bug/20241106_155823_DSC00339";
		var items = new List<FileIndexItem>
		{
			new()
			{
				FilePath = "/map_collection_bug/20241106_155758_DSC00338.arw",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new()
			{
				FilePath = "/map_collection_bug/20241106_155758_DSC00338.jpg",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/map_collection_bug/20241106_155823_DSC00339",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.directory
			},
			new()
			{
				FilePath = "/map_collection_bug/20241106_155823_DSC00339.arw",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new()
			{
				FilePath = "/map_collection_bug/20241106_155823_DSC00339.jpg",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			},
			new()
			{
				FilePath = "/map_collection_bug/20241106_155825_DSC00340.arw",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
			},
			new()
			{
				FilePath = "/map_collection_bug/20241106_155825_DSC00340.jpg",
				ParentDirectory = parentFolderPath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		_context.FileIndex.AddRange(items);
		_context.SaveChanges();

		// Act
		var result = _query.QueryGetNextPrevInFolder(parentFolderPath, currentFolder);

		// Assert
		Assert.IsNotNull(result);
		
		Assert.AreEqual(4, result.Count);
		Assert.AreEqual("/map_collection_bug/20241106_155758_DSC00338.jpg", result[0].FilePath);
		Assert.AreEqual("/map_collection_bug/20241106_155823_DSC00339", result[1].FilePath);
		Assert.AreEqual("/map_collection_bug/20241106_155823_DSC00339.jpg", result[2].FilePath);
		Assert.AreEqual("/map_collection_bug/20241106_155825_DSC00340.jpg", result[3].FilePath);
	}
}
