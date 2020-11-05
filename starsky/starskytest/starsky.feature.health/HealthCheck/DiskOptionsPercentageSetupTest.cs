using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.feature.health.HealthCheck
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
