using System;
using System.Threading.Tasks;
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
		private const string ProgramId = "starskytest";
		private const string FileTypeDescription = "Starsky Test File";

		[TestInitialize]
		public void TestInitialize()
		{
			SetupEnsureAssociationsSet();
		}

		private static CreateFakeStarskyWindowsExe SetupEnsureAssociationsSet()
		{
			if ( !new AppSettings().IsWindows )
			{
				return new CreateFakeStarskyWindowsExe();
			}

			var mock = new CreateFakeStarskyWindowsExe();
			var filePath = mock.FullFilePath;
			WindowsSetFileAssociations.EnsureAssociationsSet(
				new FileAssociation
				{
					Extension = Extension,
					ProgId = ProgramId,
					FileTypeDescription = FileTypeDescription,
					ExecutableFilePath = filePath
				});
			return mock;
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
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgramId}", false);
		}

		[TestMethod]
		public async Task Service_OpenDefault_HappyFlow__WindowsOnly()
		{
			if ( !new AppSettings().IsWindows )
			{
				Assert.Inconclusive("This test if for Windows Only");
				return;
			}

			var mock = SetupEnsureAssociationsSet();

			var result =
				new OpenApplicationNativeService().OpenDefault([mock.StarskyDotStarskyPath]);

			// retry due due multi threading
			if ( result != true )
			{
				Console.WriteLine("retry due due multi threading");
				await Task.Delay(100);
				SetupEnsureAssociationsSet();
				var service = new OpenApplicationNativeService();
				result = service.OpenDefault([mock.StarskyDotStarskyPath]);
			}

			Assert.IsTrue(result);
		}


		[TestMethod]
		public void OpenApplicationAtUrl_ZeroItems_SoFalse()
		{
			var result = new OpenApplicationNativeService().OpenApplicationAtUrl([], "app");
			Console.WriteLine($"result: {result}");
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void OpenDefault_ZeroItemsSo_False()
		{
			var result = new OpenApplicationNativeService().OpenDefault([]);
			Console.WriteLine($"result: {result}");
			Assert.IsFalse(result);
		}
	}
}
