using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.packagetelemetry.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using System.Runtime.InteropServices;

namespace starskytest.starsky.feature.packagetelemetry.Services
{
	[TestClass]
	public class DeviceIdServiceTest
	{
		private readonly AppSettings _appSettings;

		public DeviceIdServiceTest()
		{
			_appSettings = new AppSettings();
		}

		[TestMethod]
		public void DeviceId_WindowsId__WindowsOnly()
		{
			if ( !_appSettings.IsWindows )
			{
				Assert.Inconclusive("This test if for Windows Only");
				return;
			}

			var id = new DeviceIdService(new FakeSelectorStorage()).DeviceId(OSPlatform.Windows);
			Assert.IsNotNull( id );
		}
		
		// [TestMethod]
		// public void DeviceId__Linux()
		// {
		// 	if ( _appSettings.IsWindows )
		// 	{
		// 		Assert.Inconclusive("This test if for Unix Only");
		// 		return;
		// 	}
		//
		// 	var id = new DeviceIdService(new FakeSelectorStorage()).DeviceId(OSPlatform.Linux);
		// 	Assert.IsNotNull( id );
		// }
	}
}
