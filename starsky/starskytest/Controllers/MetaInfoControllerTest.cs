using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
	public sealed class MetaInfoControllerTest
	{
		private readonly IMetaInfo _metaInfo;

		public MetaInfoControllerTest()
		{
			_metaInfo = new MetaInfo(new FakeIQuery(
					new List<FileIndexItem>{new FileIndexItem("/test.jpg"), new FileIndexItem("/readonly/image.jpg"),
						new FileIndexItem("/source_missing.jpg")}), 
				new AppSettings{ ReadOnlyFolders = new List<string>{"readonly"}}, 
				new FakeSelectorStorage(new FakeIStorage(new List<string>(), 
					new List<string>{"/test.jpg","/readonly/image.jpg"}, new List<byte[]>{ 
						CreateAnImage.Bytes.ToArray(), 
						CreateAnImage.Bytes.ToArray()})),null, new FakeIWebLogger());
			
		}
		
		[TestMethod]
		public async Task Info_AllDataIncluded_WithFakeExifTool()
		{
			var controller = new MetaInfoController(_metaInfo);
			var jsonResult = await controller.InfoAsync("/test.jpg", false) as JsonResult;
			var listResult = jsonResult?.Value as List<FileIndexItem>;
			Assert.AreEqual("test, sion", listResult?.FirstOrDefault()?.Tags);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, listResult?.FirstOrDefault()?.Status);
		}

		[TestMethod]
		public async Task Info_SourceImageMissing_WithFakeExifTool()
		{
			var controller = new MetaInfoController(_metaInfo);
			var notFoundResult = await controller.InfoAsync("/source_missing.jpg") as NotFoundObjectResult;
			Assert.AreEqual(404, notFoundResult?.StatusCode);
		}
		
		[TestMethod]
		public async Task ReadOnly()
		{
			var controller = new MetaInfoController(_metaInfo)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var jsonResult = await controller.InfoAsync("/readonly/image.jpg", false) as JsonResult;

			var listResult = jsonResult?.Value as List<FileIndexItem>;
			Assert.AreEqual("test, sion", listResult?.FirstOrDefault()?.Tags);
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, listResult?.FirstOrDefault()?.Status);
		}
	}
}
