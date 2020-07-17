using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class MetaInfoControllerTest
	{
		private readonly IMetaInfo _metaInfo;
		private readonly FakeIStorage _iStorage;

		public MetaInfoControllerTest()
		{
			_iStorage = new FakeIStorage();
			_metaInfo = new MetaInfo(new FakeIQuery(
				new List<FileIndexItem>{new FileIndexItem("/test.jpg"), 
					new FileIndexItem("/source_missing.jpg")}), 
				new AppSettings(), new FakeSelectorStorage(new FakeIStorage(new List<string>(), 
					new List<string>{"/test.jpg"}, new List<byte[]>{ CreateAnImage.Bytes})));
			
		}
		
		[TestMethod]
		public void Info_AllDataIncluded_WithFakeExifTool()
		{
			var controller = new MetaInfoController(_metaInfo);
			var jsonResult = controller.Info("/test.jpg", false) as JsonResult;
			var exiftoolModel = jsonResult.Value as List<FileIndexItem>;
			Assert.AreEqual("test, sion", exiftoolModel.FirstOrDefault().Tags);
		}

		[TestMethod]
		public void Info_SourceImageMissing_WithFakeExifTool()
		{
			var controller = new MetaInfoController(_metaInfo);
			var notFoundResult = controller.Info("/source_missing.jpg") as NotFoundObjectResult;
			Assert.AreEqual(404, notFoundResult.StatusCode);
		}
	}
}

