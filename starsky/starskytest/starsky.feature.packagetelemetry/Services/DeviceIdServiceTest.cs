using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.packagetelemetry.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Medallion.Shell;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;

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
		
		[TestMethod]
		public async Task DeviceId_BsdHostIdPath()
		{
			var storage = new FakeIStorage(new List<string>{"/"});
			var storageSelector = new FakeSelectorStorage(storage);
			var deviceService = new DeviceIdService(storageSelector);
			await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("test-id"), deviceService.BsdHostIdPath);
			var id = await deviceService.DeviceId(OSPlatform.Linux);
			Assert.IsNotNull( id );
			Assert.AreEqual("6CC41D5EC590AB78CCCECF81EF167D418C309A4598E8E45FEF78039F7D9AA9FE", id );
		}
		
		[TestMethod]
		public async Task DeviceId_MachineIdPath2()
		{
			var storage = new FakeIStorage(new List<string>{"/"});
			var storageSelector = new FakeSelectorStorage(storage);
			var deviceService = new DeviceIdService(storageSelector);
			await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("should-not-use"), deviceService.BsdHostIdPath);
			await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("test-id"), deviceService.MachineIdPath2);
			var id = await deviceService.DeviceId(OSPlatform.Linux);
			Assert.IsNotNull( id );
			Assert.AreEqual("6CC41D5EC590AB78CCCECF81EF167D418C309A4598E8E45FEF78039F7D9AA9FE", id );
		}
		
		[TestMethod]
		public async Task DeviceId_DbusMachineIdPath()
		{
			var storage = new FakeIStorage(new List<string>{"/"});
			var storageSelector = new FakeSelectorStorage(storage);
			var deviceService = new DeviceIdService(storageSelector);
			await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("should-not-use"), deviceService.BsdHostIdPath);
			await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("should-not-use"), deviceService.MachineIdPath2);
			await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("test-id"), deviceService.DbusMachineIdPath);

			var id = await deviceService.DeviceId(OSPlatform.Linux);
			Assert.IsNotNull( id );
			Assert.AreEqual("6CC41D5EC590AB78CCCECF81EF167D418C309A4598E8E45FEF78039F7D9AA9FE", id );
		}
		
		[TestMethod]
		public async Task DeviceId_MacOS__UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}

			var storage = new FakeIStorage(new List<string>{"/"});
			var storageSelector = new FakeSelectorStorage(storage);
			var deviceService = new DeviceIdService(storageSelector);

			var hostFullPathFilesystem = new StorageHostFullPathFilesystem();

			var osxHostIdMockPath =
				Path.Join(new CreateAnImage().BasePath, "osx-hostid");

			var text = "#!/bin/bash \n";
			text += "echo ' ";
			text += "+-o J314sAP  <class IOPlatformExpertDevice, id 0x10000024d, registered, matched, active, busy 0 (16827 ms), retain 39>";
			text += "{";
			text += "	\"clock-frequency\" = <00366e01>";
			text += "   \"mlb-serial-number\" = <000000000000000000000000000000000000000000000000000000000000>";
			text += "   \"IONWInterrupts\" = \"IONWInterrupts\"";
			text += "  	\"model-config\" = <\"Sunway\\;MoPED=000000000000000000000000000000\">";
			text += "  	              \"device_type\" = <\"bootrom\">";
			text += "  	                       \"#size-cells\" = <02000000>";
			text += "  	 	\"IOPlatformUUID\" = \"ea49e46c-1995-4405-aa3e-3bc4f1412448\"";
			text += "  	} ";
			text += " ' ";
			
			await hostFullPathFilesystem.WriteStreamAsync(PlainTextFileHelper.StringToStream(text),osxHostIdMockPath);
			
			await Command.Run("chmod", "+x",
				osxHostIdMockPath).Task;
			deviceService.IoReg = osxHostIdMockPath;
			var id = await deviceService.DeviceId(OSPlatform.OSX);
			hostFullPathFilesystem.FileDelete(osxHostIdMockPath);
			
			Assert.IsNotNull( id );
			Assert.AreEqual("879A1F5897C5E89E6F5981EE38E2F45A7E77023A66B264A84DF831C592B8DADE", id );
		}
				
		[TestMethod]
		public async Task DeviceId_MacOS_Direct__UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}

			var storage = new FakeIStorage(new List<string>{"/"});
			var storageSelector = new FakeSelectorStorage(storage);
			var deviceService = new DeviceIdService(storageSelector);

			var hostFullPathFilesystem = new StorageHostFullPathFilesystem();

			var osxHostIdMockPath =
				Path.Join(new CreateAnImage().BasePath, "osx-hostid");

			var text = "#!/bin/bash \n";
			text += "echo ' ";
			text += "+-o J314sAP  <class IOPlatformExpertDevice, id 0x10000024d, registered, matched, active, busy 0 (16827 ms), retain 39>";
			text += "{";
			text += "	\"clock-frequency\" = <00366e01>";
			text += "   \"mlb-serial-number\" = <000000000000000000000000000000000000000000000000000000000000>";
			text += "   \"IONWInterrupts\" = \"IONWInterrupts\"";
			text += "  	\"model-config\" = <\"Sunway\\;MoPED=000000000000000000000000000000\">";
			text += "  	              \"device_type\" = <\"bootrom\">";
			text += "  	                       \"#size-cells\" = <02000000>";
			text += "  	 	\"IOPlatformUUID\" = \"ea49e46c-1995-4405-aa3e-3bc4f1412448\"";
			text += "  	} ";
			text += " ' ";
			
			await hostFullPathFilesystem.WriteStreamAsync(PlainTextFileHelper.StringToStream(text),osxHostIdMockPath);
			
			await Command.Run("chmod", "+x",
				osxHostIdMockPath).Task;
			deviceService.IoReg = osxHostIdMockPath;
			var id = await deviceService.DeviceIdOsX();
			hostFullPathFilesystem.FileDelete(osxHostIdMockPath);
			
			Assert.IsNotNull( id );
			Assert.AreEqual("ea49e46c-1995-4405-aa3e-3bc4f1412448", id );
		}

		[TestMethod]
		public async Task DeviceId_MacOS_Direct_Fail__UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}
			
			var storage = new FakeIStorage(new List<string>{"/"});
			var storageSelector = new FakeSelectorStorage(storage);
			var deviceService = new DeviceIdService(storageSelector);
			deviceService.IoReg = "ls";
			var id = await deviceService.DeviceIdOsX();
			Assert.AreEqual("not set", id );
		}
	}
}
