using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;

namespace starskytest.starsky.foundation.native.OpenApplicationNative
{

	[TestClass]
	public class OpenApplicationNativeServiceTest
	{
		private const string Extension = ".starsky";
		private const string ProgId = "starskytest";
		private const string FileTypeDescription = "Starsky Test File";

		[TestInitialize]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
	"CA1416:Validate platform compatibility", Justification = "Check does exists")]
		public void TestInitialize()
		{
			if ( !new AppSettings().IsWindows )
			{
				return;
			}

			// Ensure no keys exist before the test starts
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{Extension}", false);
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgId}", false);
		}

		[TestCleanup]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
			"CA1416:Validate platform compatibility", Justification = "Check does exists")]
		public void TestCleanup()
		{
			if ( !new AppSettings().IsWindows )
			{
				return;
			}

			// Cleanup created keys
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{Extension}", false);
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgId}", false);
		}

		[TestMethod]
		public void OpenDefault_HappyFlow__WindowsOnly()
		{
			if ( !new AppSettings().IsWindows )
			{
				Assert.Inconclusive("This test if for Windows Only");
				return;
			}

			var mock = new CreateFakeStarskyExe();
			var filePath = mock.FullFilePath;
			WindowsSetFileAssociations.EnsureAssociationsSet(
				new FileAssociation
				{
					Extension = Extension,
					ProgId = ProgId,
					FileTypeDescription = FileTypeDescription,
					ExecutableFilePath = filePath
				});

			var result = new OpenApplicationNativeService().OpenDefault([mock.StarskyDotStarskyPath]);
			Assert.IsTrue(result);
		}


		[TestMethod]
		public void OpenApplicationAtUrl_ZeroItemsSoFalse()
		{
			var result = new OpenApplicationNativeService().OpenApplicationAtUrl([], "app");
			Assert.IsFalse(result);
		}


		[TestMethod]
		public void OpenDefault_ZeroItemsSoFalse()
		{
			var result = new OpenApplicationNativeService().OpenDefault([]);
			Assert.IsFalse(result);
		}
	}
}
