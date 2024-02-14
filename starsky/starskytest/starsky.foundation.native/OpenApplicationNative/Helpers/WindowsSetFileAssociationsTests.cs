using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers
{
	[TestClass]
	public class WindowsSetFileAssociationsTests
	{
		[TestMethod]
		public void EnsureAssociationsSet()
		{
			if ( !new AppSettings().IsWindows )
			{
				Assert.Inconclusive("This test if for Windows Only");
				return;
			}
			
			var filePath = new CreateFakeStarskyWindowsExe().FullFilePath;
			WindowsSetFileAssociations.EnsureAssociationsSet(
				new FileAssociation
				{
					Extension = ".starsky",
					ProgId = "starskytest",
					FileTypeDescription = "Starsky Test File",
					ExecutableFilePath = filePath
				});

			var registryKeyPath = @"Software\Classes\starskytest\shell\open\command";

			using ( RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath) )
			{
				var value = key.GetValue(string.Empty).ToString();
			}
		}
	}
}
