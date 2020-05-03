using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class ImportHistoryControllerTest
	{
		private readonly FakeIImportQuery _fakeImportQuery;

		public ImportHistoryControllerTest()
		{
			_fakeImportQuery = new FakeIImportQuery(new List<string> {"/test.jpg"});
		}
		
		[TestMethod]
		public void HistoryTest()
		{
			var result = new ImportHistoryController(_fakeImportQuery).History() as JsonResult;
			var output = result.Value as List<ImportIndexItem>;
			Assert.AreEqual("/test.jpg", output.FirstOrDefault().FilePath);
		}
 	}
}
