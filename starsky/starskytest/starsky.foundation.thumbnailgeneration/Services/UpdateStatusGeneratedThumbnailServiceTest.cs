using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services;

[TestClass]
public class UpdateStatusGeneratedThumbnailServiceTest
{
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_NoItems()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(new List<GenerationResultModel>());

		var getResult = await query.Get();
		Assert.AreEqual(0, getResult.Count);
	}

	public static readonly ImmutableArray<GenerationResultModel> ExampleData = new ImmutableArray<GenerationResultModel>
	{
		new GenerationResultModel
		{
			FileHash = "large_1",
			SubPath = "test.jpg",
			Size = ThumbnailSize.Large,
			Success = true
		},
		new GenerationResultModel
		{
			FileHash = "large_2",
			SubPath = "test.jpg",
			Size = ThumbnailSize.Large,
			Success = false
		},
		new GenerationResultModel
		{
			FileHash = "small_1",
			SubPath = "test.jpg",
			Size = ThumbnailSize.Small,
			Success = true
		},
		new GenerationResultModel
		{
			FileHash = "small_2",
			SubPath = "test.jpg",
			Size = ThumbnailSize.Small,
			Success = false
		},
		new GenerationResultModel
		{
			FileHash = "extra_large_1",
			SubPath = "test.jpg",
			Size = ThumbnailSize.ExtraLarge,
			Success = true
		},
		new GenerationResultModel
		{
			FileHash = "extra_large_2",
			SubPath = "test.jpg",
			Size = ThumbnailSize.ExtraLarge,
			Success = false
		}
	};
	
	
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Count6()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData.ToList());

		var getResult = await query.Get();
		Assert.AreEqual(6, getResult.Count);
	}
	
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Index_0()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData.ToList());

		var getResult = await query.Get(ExampleData[0].FileHash); // see the index
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(true, getResult[0].Large);
		Assert.AreEqual(null, getResult[0].ExtraLarge);
		Assert.AreEqual(null, getResult[0].Small);
	}
		
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Index_1()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData.ToList());
								// see the index
		var getResult = await query.Get(ExampleData[1].FileHash);
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(false, getResult[0].Large);
		Assert.AreEqual(null, getResult[0].ExtraLarge);
		Assert.AreEqual(null, getResult[0].Small);
	}
	
			
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Index_2()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData.ToList());

		var getResult = await query.Get(ExampleData[2].FileHash);
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(null, getResult[0].Large);
		Assert.AreEqual(null, getResult[0].ExtraLarge);
		Assert.AreEqual(true, getResult[0].Small);
	}
				
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Index_3()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData.ToList());

		var getResult = await query.Get(ExampleData[3].FileHash);
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(null, getResult[0].Large);
		Assert.AreEqual(null, getResult[0].ExtraLarge);
		Assert.AreEqual(false, getResult[0].Small);
	}
	
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Index_4()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData.ToList());

		var getResult = await query.Get(ExampleData[4].FileHash);
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(null, getResult[0].Large);
		Assert.AreEqual(true, getResult[0].ExtraLarge);
		Assert.AreEqual(null, getResult[0].Small);
	}
				
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Index_5()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData.ToList());

		var getResult = await query.Get(ExampleData[5].FileHash);
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(null, getResult[0].Large);
		Assert.AreEqual(false, getResult[0].ExtraLarge);
		Assert.AreEqual(null, getResult[0].Small);
	}
	
	public static readonly ImmutableArray<GenerationResultModel> ExampleData2 = new ImmutableArray<GenerationResultModel>
	{
		new GenerationResultModel
		{
			FileHash = "image_01",
			Size = ThumbnailSize.Large,
			Success = true,
			SubPath = "test.jpg"
		},
		new GenerationResultModel
		{
			FileHash = "image_01",
			Size = ThumbnailSize.ExtraLarge,
			Success = true,
			SubPath = "test.jpg",
		},
		new GenerationResultModel
		{
			FileHash = "image_01",
			Size = ThumbnailSize.Small,
			Success = true,
			SubPath = "test.jpg"
		},
		new GenerationResultModel
		{
			FileHash = "image_01",
			Size = ThumbnailSize.TinyMeta,
			Success = false,
			SubPath = "test.jpg"
		}
	};
	
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Data2_NewItem()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData2.ToList());

		var getResult = await query.Get(ExampleData2[0].FileHash);
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(true, getResult[0].Large);
		Assert.AreEqual(true, getResult[0].ExtraLarge);
		Assert.AreEqual(true, getResult[0].Small);
		Assert.AreEqual(false, getResult[0].TinyMeta);
	}
	
	[TestMethod]
	public async Task UpdateStatusGeneratedThumbnailService_Data2_UpdateItem()
	{
		var query = new FakeIThumbnailQuery();
		var service = new UpdateStatusGeneratedThumbnailService(query);
		await service.AddOrUpdateStatusAsync(ExampleData2.ToList());

		await service.AddOrUpdateStatusAsync(new List<GenerationResultModel>{new GenerationResultModel
		{
			FileHash = "image_01",
			Size = ThumbnailSize.Large,
			Success = false,
			SubPath = "test.jpg"
		}});

		
		var getResult = await query.Get(ExampleData2[0].FileHash);
		Assert.AreEqual(1, getResult.Count);
		Assert.AreEqual(false, getResult[0].Large);
		Assert.AreEqual(true, getResult[0].ExtraLarge);
		Assert.AreEqual(true, getResult[0].Small);
		Assert.AreEqual(false, getResult[0].TinyMeta);
	}


}
