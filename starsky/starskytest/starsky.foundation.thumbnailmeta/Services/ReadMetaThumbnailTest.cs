using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.metathumbnail.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.readmeta.Services
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
					new WriteMetaThumbnail(selectorStorage, logger, new AppSettings()), logger)
					.AddMetaThumbnail("/no_thumbnail.jpg","anything");

			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task Image_WithThumbnail_IntegrationTest()
		{
			var selectorStorage = new FakeSelectorStorage(_iStorageFake);
			var logger = new FakeIWebLogger();
			var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage, 
					new OffsetDataMetaExifThumbnail(selectorStorage, logger), 
					new WriteMetaThumbnail(selectorStorage, logger, new AppSettings()), logger)
				.AddMetaThumbnail("/poppy.jpg","/meta_image");
			
			Assert.IsTrue(result);
			Assert.IsTrue(_iStorageFake.ExistFile("/meta_image@meta"));
		}
		
	}
}
