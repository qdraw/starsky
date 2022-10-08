using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.realtime.Formats;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Services;

namespace starskytest.starsky.foundation.settings.Services;

[TestClass]
public class SettingsServiceTest
{
	private static ApplicationDbContext SetScope()
	{
		// var provider = new ServiceCollection()
		// 	.AddMemoryCache()
		// 	.BuildServiceProvider();
		// var memoryCache = provider.GetService<IMemoryCache>();
            
		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase("test1234");
		var options = builderDb.Options;
		return new ApplicationDbContext(options);
	}

	private static async Task RemoveAsync(ApplicationDbContext dbContext, SettingsType key )
	{
		var item =await dbContext.Settings.FirstOrDefaultAsync(p => p.Key == Enum.GetName(key));
		if ( item != null )
		{
			dbContext.Remove(item);
			await dbContext.SaveChangesAsync();
		}
	}

	[TestMethod]
	public async Task GetSetting()
	{
		var dbContext = SetScope();
		var value = DateTime.UtcNow.ToString(
			SettingsFormats.LastSyncBackgroundDateTime,
			CultureInfo.InvariantCulture);
		dbContext.Settings.Add(new SettingsItem
		{
			Key = Enum.GetName(SettingsType.LastSyncBackgroundDateTime),
			Value = value
		});
		await dbContext.SaveChangesAsync();


		var item = await new SettingsService(dbContext).GetSetting(SettingsType
			.LastSyncBackgroundDateTime);
		
		Assert.AreEqual(value, item?.Value);

		await RemoveAsync(dbContext, SettingsType
			.LastSyncBackgroundDateTime);
	}
	
	[TestMethod]
	public async Task GetSettingCast_DateTime()
	{
		var dbContext = SetScope();
		var datetime = DateTime.UtcNow;
		var value = datetime.ToString(
			SettingsFormats.LastSyncBackgroundDateTime,
			CultureInfo.InvariantCulture);
		dbContext.Settings.Add(new SettingsItem
		{
			Key = Enum.GetName(SettingsType.LastSyncBackgroundDateTime),
			Value = value
		});
		await dbContext.SaveChangesAsync();

		var item = await new SettingsService(dbContext).GetSetting<DateTime>(SettingsType
			.LastSyncBackgroundDateTime);
		
		Assert.AreEqual(datetime.ToString(CultureInfo.InvariantCulture), item.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
		await RemoveAsync(dbContext, SettingsType
			.LastSyncBackgroundDateTime);
	}
	
	[TestMethod]
	public async Task GetSettingCast_String()
	{
		var dbContext = SetScope();
		dbContext.Settings.Add(new SettingsItem
		{
			Key = Enum.GetName(SettingsType.LastSyncBackgroundDateTime),
			Value = "test"
		});
		await dbContext.SaveChangesAsync();

		var item = await new SettingsService(dbContext).GetSetting<string>(SettingsType
			.LastSyncBackgroundDateTime);
		
		Assert.AreEqual("test", item);
		await RemoveAsync(dbContext, SettingsType
			.LastSyncBackgroundDateTime);
	}
	
	[TestMethod]
	public async Task GetSettingCast_DateTime_Fail()
	{
		var dbContext = SetScope();
		dbContext.Settings.Add(new SettingsItem
		{
			Key = Enum.GetName(SettingsType.LastSyncBackgroundDateTime),
			Value = "94u395035490543"
		});
		await dbContext.SaveChangesAsync();

		var item = await new SettingsService(dbContext).GetSetting<DateTime>(SettingsType
			.LastSyncBackgroundDateTime);

		DateTime defaultDatetime = default;
		Assert.AreEqual(defaultDatetime.ToString(CultureInfo.InvariantCulture), item.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
		await RemoveAsync(dbContext, SettingsType
			.LastSyncBackgroundDateTime);
	}
	
	[TestMethod]
	public async Task AddOrUpdateSetting_Null()
	{
		var dbContext = SetScope();

		var item =
			await new SettingsService(dbContext).AddOrUpdateSetting(
				new SettingsItem
				{
					Key = null,
					Value = null
				});
		Assert.IsNull(item);
	}
	
	[TestMethod]
	public async Task AddOrUpdateSetting_ItemAdded()
	{
		var dbContext = SetScope();

		var item =
			await new SettingsService(dbContext).AddOrUpdateSetting(
				new SettingsItem
				{
					Key = Enum.GetName(SettingsType.LastSyncBackgroundDateTime),
					Value = "test"
				});
		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);
		
		var dbResult = await dbContext.Settings.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(SettingsType.LastSyncBackgroundDateTime));
		Assert.AreEqual("test", dbResult?.Value);
		await RemoveAsync(dbContext, SettingsType
			.LastSyncBackgroundDateTime);
	}
	
	[TestMethod]
	public async Task AddOrUpdateSetting_ItemAdded_ViaEnum()
	{
		var dbContext = SetScope();

		var item =
			await new SettingsService(dbContext).AddOrUpdateSetting(
				SettingsType.LastSyncBackgroundDateTime, "test");
		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);
		
		var dbResult = await dbContext.Settings.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(SettingsType.LastSyncBackgroundDateTime));
		
		Assert.AreEqual("test", dbResult?.Value);
		await RemoveAsync(dbContext, SettingsType
			.LastSyncBackgroundDateTime);
	}
	
	[TestMethod]
	public async Task AddOrUpdateSetting_ItemUpdated()
	{
		var dbContext = SetScope();
		await new SettingsService(dbContext).AddOrUpdateSetting(
			new SettingsItem
			{
				Key = Enum.GetName(SettingsType.LastSyncBackgroundDateTime),
				Value = "test0"
			});
		
		var item = await new SettingsService(dbContext).AddOrUpdateSetting(
			new SettingsItem
			{
				Key = Enum.GetName(SettingsType.LastSyncBackgroundDateTime),
				Value = "test"
			});

		Assert.IsNotNull(item);
		Assert.AreEqual("test", item.Value);
		
		var dbResult = await dbContext.Settings.FirstOrDefaultAsync(p =>
			p.Key == Enum.GetName(SettingsType.LastSyncBackgroundDateTime));
		Assert.AreEqual("test", dbResult?.Value);
		await RemoveAsync(dbContext, SettingsType
			.LastSyncBackgroundDateTime);
	}
}
