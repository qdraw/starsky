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
		_context = serviceScope.CreateScope().ServiceProvider
			.GetRequiredService<ApplicationDbContext>();
		_thumbnailQuery = new ThumbnailQuery(_context, serviceScope);
	}

	private static IServiceScopeFactory CreateNewScope(string? name = null)
	{
		name ??= nameof(ThumbnailQueryTest);
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(name));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task
		AddThumbnailRangeAsync_AddNewThumbnails_ReturnsNewThumbnails()
	{
		// Arrange
		var fileHashes = new List<string> { "00123", "00456" };
		var data = new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("00123", null, true),
			new ThumbnailResultDataTransferModel("00456", null, true)
		};

		// Act
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(data);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.Where(p => p.FileHash is "00123" or "00456")
			.All(x => x.Small == true));
		Assert.IsTrue(result.Where(p => p.FileHash is "00123" or "00456")
			.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));

		var thumbnails = await _context.Thumbnails.ToListAsync();
		Assert.AreEqual(2,
			thumbnails.Count(p => p.FileHash is "00123" or "00456"));
		Assert.IsTrue(thumbnails.All(x => x.Small == true));
		Assert.IsTrue(thumbnails.Select(x => x.FileHash)
			.All(x => fileHashes.Contains(x)));
	}


	[TestMethod]
	public async Task AddThumbnailRangeAsync_Disposed_Success()
	{
		// Arrange
		var fileHashes = new List<string> { "627445", "8127445" };
		var data = new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("627445", null, null,
				true),
			new ThumbnailResultDataTransferModel("8127445", null, null,
				true)
		};

		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider
			.GetRequiredService<ApplicationDbContext>();
		var thumbnailQuery = new ThumbnailQuery(dbContext, serviceScope);

		// And dispose
		await dbContext.DisposeAsync();

		// Act
		var result = await thumbnailQuery.AddThumbnailRangeAsync(data);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.Where(p => p.FileHash is "627445" or "8127445")
			.All(x => x.Large == true));
		Assert.IsTrue(result.Where(p => p.FileHash is "627445" or "8127445")
			.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));

		var thumbnails = await _context.Thumbnails.Where(p =>
			p.FileHash == "627445" ||
			p.FileHash == "8127445").ToListAsync();
		Assert.AreEqual(2,
			thumbnails.Count(p => p.FileHash is "627445" or "8127445"));
		Assert.IsTrue(thumbnails.All(x => x.Large == true));
		Assert.IsTrue(thumbnails.Select(x => x.FileHash)
			.All(x => fileHashes.Contains(x)));
	}

	[TestMethod]
	[ExpectedException(typeof(ObjectDisposedException))]
	public async Task AddThumbnailRangeAsync_Disposed_NoServiceScope()
	{
		// Arrange
		var data = new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("627445", null, null,
				true),
			new ThumbnailResultDataTransferModel("8127445", null, null,
				true)
		};

		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "Add_writes_to_database11")
			.Options;

		var dbContext = new ApplicationDbContext(options);
		var thumbnailQuery =
			new ThumbnailQuery(dbContext, null); // <-- no service scope

		// And dispose
		await dbContext.DisposeAsync();

		// Act
		await thumbnailQuery.AddThumbnailRangeAsync(data);
		// no service scope so exception
	}

	[TestMethod]
	public async Task
		AddThumbnailRangeInternalAsync_Updates_Existing_Thumbnails_In_Database()
	{
		// Arrange
		var sizes = new List<ThumbnailSize>
		{
			ThumbnailSize.Small, ThumbnailSize.Large,
		};

		_context.Thumbnails.AddRange(sizes.Select(size => new ThumbnailItem(
			"file" + size, null,
			size == ThumbnailSize.Small, size == ThumbnailSize.Large, null)));
		await _context.SaveChangesAsync();

		// Act
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel(
					"file" + ThumbnailSize.Small, null, true),
				new ThumbnailResultDataTransferModel(
					"file" + ThumbnailSize.Large, null, null, true),
			});

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.AreEqual(2, _context.Thumbnails.Count(p =>
			p.FileHash == "file" + ThumbnailSize.Small
			|| p.FileHash == "file" + ThumbnailSize.Large));

		var dbResult = _context.Thumbnails.Where(p =>
			p.FileHash == "file" + ThumbnailSize.Small
			|| p.FileHash == "file" + ThumbnailSize.Large);
		Assert.IsTrue(result
			.Where(p => p.FileHash == "file" + ThumbnailSize.Small)
			.All(x => x.Small == true));
		Assert.IsTrue(result
			.Where(p => p.FileHash == "file" + ThumbnailSize.Large)
			.All(x => x.Large == true));

		Assert.IsTrue(dbResult
			.Where(p => p.FileHash == "file" + ThumbnailSize.Small)
			.All(x => x.Small == true));
		Assert.IsTrue(dbResult
			.Where(p => p.FileHash == "file" + ThumbnailSize.Large)
			.All(x => x.Large == true));
	}

	[TestMethod]
	public async Task
		AddThumbnailRangeAsync_UpdateExistingThumbnails_ReturnsUpdatedThumbnails()
	{
		// Arrange
		var fileHashes = new List<string> { "9123", "9456" };

		var thumbnail = new ThumbnailItem("9123", null, true, null, null);
		_context.Thumbnails.Add(thumbnail);
		await _context.SaveChangesAsync();

		// Act
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel("9123", null, true,true),
				new ThumbnailResultDataTransferModel("9456", null, true),
			});

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.All(x => x.Small == true));
		Assert.IsTrue(result.Select(x => x.FileHash)
			.All(x => fileHashes.Contains(x)));

		var thumbnails = await _context.Thumbnails.ToListAsync();
		Assert.AreEqual(2,
			thumbnails.Count(p => p.FileHash is "9123" or "9456"));
		Assert.IsTrue(thumbnails.Where(p => p.FileHash is "9123" or "9456")
			.All(x => x.Small == true));
		Assert.IsTrue(thumbnails.Where(p => p.FileHash is "9123" or "9456")
			.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}

	[TestMethod]
	public async Task
		AddThumbnailRangeAsync_AlreadyExistingThumbnailItems_ReturnsNull()
	{
		// Arrange
		var fileHashes = new List<string> { "789", "1011" };

		// Act
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel("789", null, true),
				new ThumbnailResultDataTransferModel("1011", null, true),
			});

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.All(x => x.Small == true));
		Assert.IsTrue(result.Select(x => x.FileHash)
			.All(x => fileHashes.Contains(x)));
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public async Task AddThumbnailRangeAsync_NullFileHashes_ArgumentException()
	{
		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel(null!)
			});
	}


	[TestMethod]
	public async Task AddThumbnailRangeAsync_NoContent()
	{
		var result = await _thumbnailQuery.AddThumbnailRangeAsync(new List<ThumbnailResultDataTransferModel>());
		Assert.AreEqual(0,result?.Count);
	}
	
	
	[TestMethod]
	public async Task
		AddThumbnailRangeAsync_UpdateInsteadOfOverwrite()
	{
		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel("9475", true)
				{
					Reasons = "test"
				},
			});

		await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel("9475", null, true, true, true)
				{
					Reasons = "test"
				}
			});
		
		// Assert
		var result3 = await _thumbnailQuery.Get("9475");

		var item = result3.FirstOrDefault();
		
		Assert.IsNotNull(item);
		Assert.AreEqual(true, item?.TinyMeta);
		Assert.AreEqual(true, item?.Small);
		Assert.AreEqual(true, item?.Large);
		Assert.AreEqual(true, item?.ExtraLarge);
		Assert.AreEqual("test", item?.Reasons);
	}
	
	[TestMethod]
	public async Task
		AddThumbnailRangeAsync_UpdateInsteadOfOverwrite2()
	{
		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel("457838", true)
				{
					Reasons = "word"
				},
			});

		await _thumbnailQuery.AddThumbnailRangeAsync(
			new List<ThumbnailResultDataTransferModel>
			{
				new ThumbnailResultDataTransferModel("457838", null, true, true, true)
				{
					Reasons = "test2"
				}
			});
		
		// Assert
		var result3 = await _thumbnailQuery.Get("457838");

		var item = result3.FirstOrDefault();
		
		Assert.IsNotNull(item);
		Assert.AreEqual(true, item?.TinyMeta);
		Assert.AreEqual(true, item?.Small);
		Assert.AreEqual(true, item?.Large);
		Assert.AreEqual(true, item?.ExtraLarge);
		Assert.AreEqual("test2,word", item?.Reasons);
	}

	[TestMethod]
	public async Task CheckForDuplicates_NewThumbnails_ReturnsNewThumbnails()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("1213", null, true, null, null),
			new ThumbnailItem("1516", null, true, null, null),
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
			new ThumbnailItem("1213", null, true, null, null),
			// duplicate item
			new ThumbnailItem("1213", null, true, null, null),
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
	public async Task CheckForDuplicates_NewThumbnails_equalContent()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("347598453", null, true, null, null),
		};
		
		_context.Thumbnails.Add(items[0]!);
		await _context.SaveChangesAsync();
		
		// Act
		// run for a second time
		var result = await ThumbnailQuery.CheckForDuplicates(_context, items);

		// Assert
		Assert.IsNotNull(result.equalThumbnailItems);
		Assert.AreEqual(1, result.equalThumbnailItems.Count);
		Assert.IsTrue(result.equalThumbnailItems.All(x => x.Small == true));
		Assert.IsTrue(result.equalThumbnailItems.Select(x => x.FileHash).All(x => new List<string> { "347598453" }.Contains(x)));
	}
	
	[TestMethod]
	public async Task RemoveThumbnails_ShouldRemove()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("3478534758", null, true, null, null),
		};
		
		_context.Thumbnails.Add(items[0]!);
		await _context.SaveChangesAsync();
		
		// Act
		var query = new ThumbnailQuery(_context, null!);
		await query.RemoveThumbnails(new List<string>{"3478534758"});

		// Assert
		var getter = await query.Get("3478534758");
		Assert.AreEqual(0,getter.Count);
	}
	
	[TestMethod]
	public async Task RemoveThumbnails_ShouldRemove2()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("9086798654", null, true, null, null),
			new ThumbnailItem("9607374598453", null, true, null, null),

		};
		
		_context.Thumbnails.Add(items[0]!);
		await _context.SaveChangesAsync();
		
		// Act
		var query = new ThumbnailQuery(_context, null!);
		await query.RemoveThumbnails(new List<string>{"9086798654","9607374598453"});

		// Assert
		var getter = await query.Get("9607374598453");
		Assert.AreEqual(0,getter.Count);
	}
	
	[TestMethod]
	public async Task RemoveThumbnails_ZeroItems()
	{
		// Act
		var query = new ThumbnailQuery(_context, null!);
		await query.RemoveThumbnails(new List<string>());

		// Assert
		var getter = await query.Get("3787453");
		Assert.AreEqual(0,getter.Count);
	}
 
	[TestMethod]
	public async Task Get_Test_ReturnOne()
	{
		// Arrange
		var fileHashes = new List<string> { "457838754" };
	
		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("457838754", null, true)
		});
	
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
		var fileHashes = new List<string> { "3456789" };
	
		// Act
		await _thumbnailQuery.AddThumbnailRangeAsync(new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("3456789", null, true)
		});	
		
		// Assert
		var thumbnails = await _thumbnailQuery.Get();
		Assert.AreEqual(true, thumbnails.Count >= 1);
		Assert.AreEqual(true, thumbnails.Count(p => p.FileHash == "3456789") == 1);
	
		Assert.IsTrue(thumbnails.Where(p => p.FileHash == "3456789")
			.All(x => x.Small == true));
		Assert.IsTrue(thumbnails.Where(p => p.FileHash == "3456789")
			.Select(x => x.FileHash).All(x => fileHashes.Contains(x)));
	}

	[TestMethod]
	public async Task CheckForDuplicates_UpdateExistingThumbnails_ReturnsUpdatedThumbnails()
	{
		// Arrange
		var items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("1718", null, true, null, null),
			new ThumbnailItem("1920", null, true, null, null),
		};
	
		await ThumbnailQuery.CheckForDuplicates(_context, items);
	
		items = new List<ThumbnailItem?>
		{
			new ThumbnailItem("1718", null, null, true, null),
			new ThumbnailItem("1920", null, null, true, null),
		};
	
		// Act
		var result = await ThumbnailQuery.CheckForDuplicates(_context, items);
	
		// Assert
		Assert.IsNotNull(result.updateThumbnailItems);
		Assert.AreEqual(2, result.Item1.Count);
		Assert.IsTrue(result.updateThumbnailItems.All(x => x.Large == false));
		Assert.IsTrue(result.updateThumbnailItems.Select(x => x.FileHash)
			.All(x => new List<string> { "123", "456" }.Contains(x)));
	}
	
	[TestMethod]
	public async Task RenameAsync_ShouldRename()
	{
		// Act
		var query = new ThumbnailQuery(_context, null!);
		await _thumbnailQuery.AddThumbnailRangeAsync(new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("3787453", null, true)
		});	

		// Assert
		var getter = await query.RenameAsync("3787453","__new__hash__");
		Assert.AreEqual(true,getter);
		
		var getter2 = await query.Get("__new__hash__");
		Assert.AreEqual(1,getter2.Count);
	}
		
	[TestMethod]
	public async Task RenameAsync_NotFound()
	{
		// Act
		var query = new ThumbnailQuery(_context, null!);

		// Assert
		var getter = await query.RenameAsync("not-found","__new__hash__");
		Assert.AreEqual(false,getter);
	}
	
	[TestMethod]
	public async Task RenameAsync_ShouldOverwrite()
	{
		// Act
		var query = new ThumbnailQuery(_context, null!);
		await _thumbnailQuery.AddThumbnailRangeAsync(new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("357484875", null, true)
		});	

		// Assert
		var getter = await query.RenameAsync("357484875","357484875");
		Assert.AreEqual(true,getter);
		
		var getter2 = await query.Get("357484875");
		Assert.AreEqual(1,getter2.Count);
	}


	[TestMethod]
	public async Task UnprocessedGeneratedThumbnails_EmptyDb()
	{
		var serviceScope = CreateNewScope("UnprocessedGeneratedThumbnails1");
		var context = serviceScope.CreateScope().ServiceProvider
			.GetRequiredService<ApplicationDbContext>();
		
		var query = new ThumbnailQuery(context, null!);
		var result = await query.UnprocessedGeneratedThumbnails();
		Assert.AreEqual(0,result.Count);
	}
	
	[TestMethod]
	public async Task UnprocessedGeneratedThumbnails_OneResult()
	{
		var serviceScope = CreateNewScope("UnprocessedGeneratedThumbnails2");
		var context = serviceScope.CreateScope().ServiceProvider
			.GetRequiredService<ApplicationDbContext>();
		
		context.Thumbnails.Add(new ThumbnailItem("123", null, true, null, null));
		await context.SaveChangesAsync();
		
		var query = new ThumbnailQuery(context, null!);
		var result = await query.UnprocessedGeneratedThumbnails();
		Assert.AreEqual(1,result.Count);
	}
	
	[TestMethod]
	public async Task UnprocessedGeneratedThumbnails_OneResultOfTwo()
	{
		var serviceScope = CreateNewScope("UnprocessedGeneratedThumbnails3");
		var context = serviceScope.CreateScope().ServiceProvider
			.GetRequiredService<ApplicationDbContext>();
		
		context.Thumbnails.Add(new ThumbnailItem("1234", null, true, null, null));
		context.Thumbnails.Add(new ThumbnailItem("12355", null, true, true, true));

		await context.SaveChangesAsync();
		
		var query = new ThumbnailQuery(context, null!);
		var result = await query.UnprocessedGeneratedThumbnails();
		Assert.AreEqual(1,result.Count);
	}
}
