using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.Health;
using starskycore.Models;

namespace starskytest.Health
{
	[TestClass]
	public class DiskOptionsPercentageSetupTest
	{
		
		[TestMethod]
		public void SetupTest()
		{
			var appSettings = new AppSettings();
			var diskOptions = new DiskStorageOptions();
			new DiskOptionsPercentageSetup().Setup(appSettings.TempFolder,diskOptions);

			var value = diskOptions.GetType().GetRuntimeFields().FirstOrDefault();
			Assert.IsNotNull(value);
		}
	}
}
