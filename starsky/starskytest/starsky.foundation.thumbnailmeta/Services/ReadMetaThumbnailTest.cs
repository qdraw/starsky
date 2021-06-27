using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.metathumbnail.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.Services
{
	[TestClass]
	public class MetaExifThumbnailServiceTest
	{
		private readonly FakeIStorage _iStorageFake;
		private readonly string _exampleHash;

		public MetaExifThumbnailServiceTest()
		{
			_iStorageFake = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/no_thumbnail.jpg", "/poppy.jpg"},
				new List<byte[]>{CreateAnImage.Bytes, new CreateAnImageWithThumbnail().Bytes}
				);
			
			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/no_thumbnail.jpg").Key;
		}
		
		[TestMethod]
		public async Task NoThumbnail_InMemoryIntegration()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new OffsetDataMetaExifThumbnail(selectorStorage, logger), 
					new WriteMetaThumbnailService(selectorStorage, logger, new AppSettings()), logger)
					.AddMetaThumbnail("/no_thumbnail.jpg","anything");

			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task Image_WithThumbnail_InMemoryIntegrationTest()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new OffsetDataMetaExifThumbnail(selectorStorage, logger), 
					new WriteMetaThumbnailService(selectorStorage, logger, new AppSettings()), logger)
				.AddMetaThumbnail("/poppy.jpg","/meta_image");
			
			Assert.IsTrue(result);
			Assert.IsTrue(_iStorageFake.ExistFile("/meta_image@meta"));
		}
		
		[TestMethod]
		public async Task AddMetaThumbnail_stringString_Fake_HappyFlow()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			
			
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(), logger)
				.AddMetaThumbnail("/poppy.jpg","/meta_image");
			
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task AddMetaThumbnail_stringString_Fake_NoHashIncluded()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			
			
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(), logger)
				.AddMetaThumbnail("/poppy.jpg",null);
			
			Assert.IsTrue(result);
		}

		
		[TestMethod]
		public async Task AddMetaThumbnail_stringString_Fake_NotFound()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			
			
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(), logger)
				.AddMetaThumbnail("/not-found.jpg","/meta_image");
			
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task AddMetaThumbnail_Fake_SingleString_File()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			
			
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(), logger)
				.AddMetaThumbnail("/poppy.jpg");
			
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task AddMetaThumbnail_Fake_SingleString_Folder()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			
			
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(), logger)
				.AddMetaThumbnail("/");
			
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task AddMetaThumbnail_Fake_SingleString_NotFound()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(), logger)
				.AddMetaThumbnail("/not_found.jpg");
			
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task AddMetaThumbnail_Fake_IEnumerableString_NotFound()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(), logger)
				.AddMetaThumbnail(new List<(string, string)>{("/not_found.jpg", "hash")});
			
			Assert.IsTrue(result);
		}
	}
}
