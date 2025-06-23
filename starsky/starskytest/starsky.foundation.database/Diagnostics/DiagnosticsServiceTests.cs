using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Diagnostics;
using starsky.foundation.database.Models;
using starsky.foundation.settings.Formats;

namespace starskytest.starsky.foundation.database.Diagnostics;

[TestClass]
public class DiagnosticsServiceTests
{
	private static ApplicationDbContext SetScope()
	{
		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase("test1234");
		var options = builderDb.Options;
		return new ApplicationDbContext(options);
	}

	private static async Task RemoveAsync(ApplicationDbContext dbContext, DiagnosticsType key)
	{
		var item = await dbContext.Diagnostics.FirstOrDefaultAsync(p => p.Key == Enum.GetName(key));
		if ( item != null )
		{
			dbContext.Remove(item);
			await dbContext.SaveChangesAsync();
		}
	}

	[TestMethod]
	public async Task GetItem()
	{
		var dbContext = SetScope();
		var value = DateTime.UtcNow.ToDefaultSettingsFormat();
		dbContext.Diagnostics.Add(new DiagnosticsItem
		{
			Key = Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes)!,
			Value = value
		});
		await dbContext.SaveChangesAsync();

		var item = await new DiagnosticsService(dbContext, null!).GetItem(DiagnosticsType
			.ApplicationStoppingLifetimeInMinutes);

		Assert.AreEqual(value, item?.Value);

		await RemoveAsync(dbContext, DiagnosticsType
			.ApplicationStoppingLifetimeInMinutes);
	}

	[TestMethod]
	public async Task AddOrUpdateItem_Null()
	{
		var dbContext = SetScope();

		var item =
			await new DiagnosticsService(dbContext, null!).AddOrUpdateItem(
				new DiagnosticsItem { Key = null!, Value = null! });
		Assert.IsNull(item);
	}

	[TestMethod]
	public async Task AddOrUpdateItem_Null2()
	{
		var dbContext = SetScope();

		var input = new TestOverWriteEnumModel
		{
			Value = DiagnosticsType.ApplicationStoppingLifetimeInMinutes
		};

		// Use reflection to set the updateStatus field to UpdateAvailable
		// overwrite enum value
		var propertyInfo = input.GetType().GetProperty("Value");
		Assert.IsNotNull(propertyInfo);
		propertyInfo.SetValue(input, 44, null); // <-- this could not happen

		var item =
			await new DiagnosticsService(dbContext, null!).AddOrUpdateItem(input.Value, null!);
		Assert.IsNull(item);
	}

	[TestMethod]
	public async Task AddOrUpdateItem_ItemAdded()
	{
		var dbContext = SetScope();

		var item =
			await new DiagnosticsService(dbContext, null!).AddOrUpdateItem(
				new DiagnosticsItem
				{
					Key = Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes)!,
					Value = "test"
				});
		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);

		var dbResult = await dbContext.Diagnostics.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes));
		Assert.AreEqual("test", dbResult?.Value);
		await RemoveAsync(dbContext, DiagnosticsType
			.ApplicationStoppingLifetimeInMinutes);
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddMemoryCache();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(DiagnosticsServiceTests)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task AddOrUpdateItem_ItemAdded_Disposed()
	{
		var dbContext = SetScope();
		var scopeFactory = CreateNewScope().CreateScope().ServiceProvider
			.GetRequiredService<IServiceScopeFactory>();

		await dbContext.DisposeAsync();

		var item =
			await new DiagnosticsService(dbContext, scopeFactory).AddOrUpdateItem(
				new DiagnosticsItem
				{
					Key = Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes)!,
					Value = "test"
				});
		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);

		// restore disposed state
		dbContext = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<ApplicationDbContext>();

		var dbResult = await dbContext.Diagnostics.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes));
		Assert.AreEqual("test", dbResult?.Value);

		await RemoveAsync(dbContext, DiagnosticsType
			.ApplicationStoppingLifetimeInMinutes);
	}

	[TestMethod]
	public async Task AddOrUpdateItem_ItemAdded_ViaEnum()
	{
		var dbContext = SetScope();

		var item =
			await new DiagnosticsService(dbContext, null!).AddOrUpdateItem(
				DiagnosticsType.ApplicationStoppingLifetimeInMinutes, "test");
		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);

		var dbResult = await dbContext.Diagnostics.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes));

		Assert.AreEqual("test", dbResult?.Value);
		await RemoveAsync(dbContext, DiagnosticsType
			.ApplicationStoppingLifetimeInMinutes);
	}

	[TestMethod]
	public async Task AddOrUpdateItem_ItemUpdated_Disposed()
	{
		var scopeFactory = CreateNewScope().CreateScope().ServiceProvider
			.GetRequiredService<IServiceScopeFactory>();
		var dbContext = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<ApplicationDbContext>();

		await dbContext.DisposeAsync();

		await new DiagnosticsService(dbContext, scopeFactory).AddOrUpdateItem(
			new DiagnosticsItem
			{
				Key = Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes)!,
				Value = "test0"
			});

		var item = await new DiagnosticsService(dbContext, scopeFactory).AddOrUpdateItem(
			new DiagnosticsItem
			{
				Key = Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes)!,
				Value = "test"
			});

		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);

		// restore disposed state
		dbContext = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<ApplicationDbContext>();

		var dbResult = await dbContext.Diagnostics.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes));
		Assert.AreEqual("test", dbResult?.Value);

		await RemoveAsync(dbContext, DiagnosticsType
			.ApplicationStoppingLifetimeInMinutes);
	}

	[TestMethod]
	public async Task AddOrUpdateItem_ItemUpdated()
	{
		var dbContext = SetScope();
		await new DiagnosticsService(dbContext, null!).AddOrUpdateItem(
			new DiagnosticsItem
			{
				Key = Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes)!,
				Value = "test0"
			});

		var item = await new DiagnosticsService(dbContext, null!).AddOrUpdateItem(
			new DiagnosticsItem
			{
				Key = Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes)!,
				Value = "test"
			});

		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);

		var dbResult = await dbContext.Diagnostics.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(DiagnosticsType.ApplicationStoppingLifetimeInMinutes));
		Assert.AreEqual("test", dbResult?.Value);
		await RemoveAsync(dbContext, DiagnosticsType
			.ApplicationStoppingLifetimeInMinutes);
	}
}

public class TestOverWriteEnumModel
{
	public DiagnosticsType Value { get; set; }
}
