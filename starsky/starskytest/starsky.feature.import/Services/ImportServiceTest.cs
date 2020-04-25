using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starskycore.Models;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.import.Services
{
	[TestClass]
	public class ImportServiceTest
	{
		private readonly FakeIStorage _iStorageFake;
		private readonly string _exampleHash;

		public ImportServiceTest()
		{
			_iStorageFake = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
				);
			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg").Key;
		}

		[TestMethod]
		public async Task Preflight_SingleImage()
		{
			var storage = new FakeIStorage();
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings), null);

			var result = await importService.Preflight(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
		} 
		
	}
}
