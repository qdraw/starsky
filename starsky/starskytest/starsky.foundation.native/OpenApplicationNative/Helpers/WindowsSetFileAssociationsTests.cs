using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;
using System.Text.RegularExpressions;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers
{
	[TestClass]
	public class WindowsSetFileAssociationsTests
	{
		private const string Extension = ".starsky";
		private const string ProgId = "starskytest";
		private const string FileTypeDescription = "Starsky Test File";

		[TestInitialize]
		public void TestInitialize()
		{
			CleanSetup();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			CleanSetup();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
			"CA1416:Validate platform compatibility", Justification = "Check does exists")]
		private static void CleanSetup()
		{
			if ( !new AppSettings().IsWindows )
			{
				return;
			}

			// Ensure no keys exist before the test starts
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{Extension}", false);
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgId}", false);
		}

		[TestMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", 
			"CA1416:Validate platform compatibility", Justification = "Check if test for windows only")]
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
					Extension = Extension,
					ProgId = ProgId,
					FileTypeDescription = FileTypeDescription,
					ExecutableFilePath = filePath
				});

			var registryKeyPath = $@"Software\Classes\{ProgId}\shell\open\command";

			using var key = Registry.CurrentUser.OpenSubKey(registryKeyPath);
			
			var valueKey = key?.GetValue(string.Empty)?.ToString();
			var pattern = "\"([^\"]*)\"";
			Assert.IsNotNull( valueKey );
			var match = Regex.Match(valueKey, pattern);
			var value = match.Groups[1].Value;

			Assert.AreEqual(filePath, value);
		}
	}
}
