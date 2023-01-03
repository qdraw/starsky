#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Thumbnails;
using starsky.foundation.platform.Enums;

namespace starskytest.starsky.foundation.database.Thumbnails;

[TestClass]
public class ThumbnailQueryTest
{
	private readonly ApplicationDbContext _context;
	private readonly ThumbnailQuery _thumbnailQuery;

	public ThumbnailQueryTest()
	{
		var serviceScope = CreateNewScope();
		_context = serviceScope.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
		_thumbnailQuery = new ThumbnailQuery(_context, serviceScope);
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(ThumbnailQueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}
	
	[TestMethod]
	public async Task AddThumbnailRangeAsync_AddNewThumbnails_ReturnsNewThumbnails()
	{
		// Arrange
		const ThumbnailSize size = ThumbnailSize.Small;
		var fileHashes = new List<string> { "00123", "00456" };

		// Act
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(size, fileHashes, true);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.Where(p => p.FileHash is "00123" or "00456").All(x => x.Small == true));
		Assert.IsTrue(result.Where(p => p.FileHash is "00123" or "00456").Select(x => x.FileHash).All(x => fileHashes.Contains(x)));

		var thumbnails = await _context.Thumbnails.ToListAsync();
		Assert.AreEqual(2, thumbnails.Count(p => p.FileHash is "00123" or "00456"));
		Assert.IsTrue(thumbnails.All(x => x.Small == true));
		Assert.IsTrue(thumbnails.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}
	
	        
	[TestMethod]
	public async Task AddThumbnailRangeAsync_Disposed_Success()
	{
		// Arrange
		var fileHashes = new List<string> { "627445", "8127445" };
		const ThumbnailSize size = ThumbnailSize.Large;

		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var thumbnailQuery = new ThumbnailQuery(dbContext,serviceScope);

		// And dispose
		await dbContext.DisposeAsync();

		// Act
		var result = await thumbnailQuery.AddThumbnailRangeAsync(size, fileHashes, true);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.Where(p => p.FileHash is "627445" or "8127445").All(x => x.Large == true));
		Assert.IsTrue(result.Where(p => p.FileHash is "627445" or "8127445").Select(x => x.FileHash).All(x => fileHashes.Contains(x)));

		var thumbnails = await _context.Thumbnails.Where(p => p.FileHash == "627445" || p.FileHash == "8127445").ToListAsync();
		Assert.AreEqual(2, thumbnails.Count(p => p.FileHash is "627445" or "8127445"));
		Assert.IsTrue(thumbnails.All(x => x.Large == true));
		Assert.IsTrue(thumbnails.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}
	
	[TestMethod]
	[ExpectedException(typeof(ObjectDisposedException))]
	public async Task AddThumbnailRangeAsync_Disposed_NoServiceScope()
	{
		// Arrange
		var fileHashes = new List<string> { "627445", "8127445" };
		const ThumbnailSize size = ThumbnailSize.Large;

		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "Add_writes_to_database11")
			.Options;

		var dbContext = new ApplicationDbContext(options);
		var thumbnailQuery = new ThumbnailQuery(dbContext,null); // <-- no service scope

		// And dispose
		await dbContext.DisposeAsync();

		// Act
		await thumbnailQuery.AddThumbnailRangeAsync(size, fileHashes, true);
		// no service scope so exception
	}

	[TestMethod]
	public async Task AddThumbnailRangeAsync_UpdateExistingThumbnails_ReturnsUpdatedThumbnails()
	{
		// Arrange
		const ThumbnailSize size = ThumbnailSize.Small;
		var fileHashes = new List<string> { "9123", "9456" };

		var thumbnail = new ThumbnailItem("9123", size, true);
		_context.Thumbnails.Add(thumbnail);
		await _context.SaveChangesAsync();

		// Act
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(size, fileHashes, true);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.All(x => x.Small == true));
		Assert.IsTrue(result.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));

		var thumbnails = await _context.Thumbnails.ToListAsync();
		Assert.AreEqual(2, thumbnails.Count(p => p.FileHash is "9123" or "9456"));
		Assert.IsTrue(thumbnails.Where(p => p.FileHash is "9123" or "9456").All(x => x.Small == true));
		Assert.IsTrue(thumbnails.Where(p => p.FileHash is "9123" or "9456").Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}
        
	[TestMethod]
	public async Task AddThumbnailRangeAsync_AlreadyExistingThumbnailItems_ReturnsNull()
	{
		// Arrange
		var size = ThumbnailSize.Small;
		var fileHashes = new List<string> { "789", "1011" };

		// Act
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(size, fileHashes, true);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.All(x => x.Small == true));
		Assert.IsTrue(result.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}
        
	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public async Task AddThumbnailRangeAsync_NullFileHashes_ArgumentException()
	{
		// Arrange
		const ThumbnailSize size = ThumbnailSize.Small;

		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(size, null!, true);
	}
	
	[TestMethod]
	public async Task CheckForDuplicates_NewThumbnails_ReturnsNewThumbnails()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("1213", ThumbnailSize.Small, true),
			new ThumbnailItem("1516", ThumbnailSize.Small, true)
		};

		// Act
		var result = await ThumbnailQuery.CheckForDuplicates(_context, items);

		// Assert
		Assert.IsNotNull(result.newThumbnailItems);
		Assert.AreEqual(2, result.newThumbnailItems.Count);
		Assert.IsTrue(result.newThumbnailItems.All(x => x.Small == true));
		Assert.IsTrue(result.Item1.Select(x => x.FileHash).All(x => new List<string> { "1213", "1516" }.Contains(x)));
	}
	
	[TestMethod]
	public async Task CheckForDuplicates_Nullable()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			null!
		};

		// Act
		var result = await ThumbnailQuery.CheckForDuplicates(_context, items);

		// Assert
		Assert.IsNotNull(result.newThumbnailItems);
		Assert.AreEqual(0, result.newThumbnailItems.Count);
	}
	
	[TestMethod]
	public async Task CheckForDuplicates_Duplicates()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("1213", ThumbnailSize.Small, true),
			new ThumbnailItem("1213", ThumbnailSize.Small, true),
		};

		// Act
		var result = await ThumbnailQuery.CheckForDuplicates(_context, items);

		// Assert
		Assert.IsNotNull(result.newThumbnailItems);
		Assert.AreEqual(1, result.newThumbnailItems.Count);
		Assert.IsTrue(result.newThumbnailItems.All(x => x.Small == true));
		Assert.IsTrue(result.Item1.Select(x => x.FileHash).All(x => new List<string> { "1213" }.Contains(x)));
	}
	
		
	[TestMethod]
	public async Task Get_Test_ReturnOne()
	{
		// Arrange
		const ThumbnailSize size = ThumbnailSize.Small;
		var fileHashes = new List<string> { "457838754" };

		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(size, fileHashes, true);

		// Assert
		var thumbnails = await _thumbnailQuery.Get("457838754");
		Assert.AreEqual(1, thumbnails.Count);
		Assert.IsTrue(thumbnails.All(x => x.Small == true));
		Assert.IsTrue(thumbnails.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}
	
		
	[TestMethod]
	public async Task Get_Test2_ReturnAll()
	{
		// Arrange
		const ThumbnailSize size = ThumbnailSize.Small;
		var fileHashes = new List<string> { "3456789" };

		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(size, fileHashes, true);

		// Assert
		var thumbnails = await _thumbnailQuery.Get();
		Assert.AreEqual(true, thumbnails.Count >= 1);
		Assert.AreEqual(true, thumbnails.Count(p => p.FileHash == "3456789") == 1);

		Assert.IsTrue(thumbnails.Where(p => p.FileHash == "3456789").All(x => x.Small == true));
		Assert.IsTrue(thumbnails.Where(p => p.FileHash == "3456789").Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}
	
	[TestMethod]
	public async Task CheckForDuplicates_UpdateExistingThumbnails_ReturnsUpdatedThumbnails()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("1718", ThumbnailSize.Small, true),
			new ThumbnailItem("1920", ThumbnailSize.Small, true)
		};

		await ThumbnailQuery.CheckForDuplicates(_context, items);

		items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("1718", ThumbnailSize.Large, false),
			new ThumbnailItem("1920", ThumbnailSize.Large, false)
		};

		// Act
		var result = await ThumbnailQuery.CheckForDuplicates(_context, items);

		// Assert
		Assert.IsNotNull(result.alreadyExistingThumbnailItems);
		Assert.AreEqual(2, result.Item1.Count);
		Assert.IsTrue(result.alreadyExistingThumbnailItems.All(x => x.Large == false));
		Assert.IsTrue(result.alreadyExistingThumbnailItems.Select(x => x.FileHash).All(x => new List<string> { "123", "456" }.Contains(x)));
	}
}
