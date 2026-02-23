using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

/// <summary>
///     Only for Windows - test the WindowsSetFileAssociationsWindows
/// </summary>
[TestClass]
public partial class WindowsSetFileAssociationsTests
{
	private const string Extension = ".starsky";
	private const string ProgId = "starskytest";
	private const string FileTypeDescription = "Starsky Test File";

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public TestContext TestContext { get; set; }

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

	[SuppressMessage("Interoperability",
		"CA1416:Validate platform compatibility", Justification = "Check does exists")]
	private static void CleanSetup()
	{
		if ( !new AppSettings().IsWindows )
		{
			return;
		}

		RetryHelper.Do(DeleteKeys, TimeSpan.FromSeconds(1));
		return;

		// 	Ensure no keys exist before the test starts
		static bool DeleteKeys()
		{
			try
			{
				Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{Extension}", false);
				Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{ProgId}", false);
				return true;
			}
			catch ( IOException )
			{
				// Ignore if the key does not exist
			}

			return false;
		}
	}

	[TestMethod]
	public async Task WindowsSetFileAssociations_EnsureAssociationsSet()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var filePath = new CreateFakeStarskyWindowsExe().FullFilePath;
		Set();

		var valueKey = GetRegistryValue();

		// Retry a few times because registry propagation can be slow on CI
		for ( var attempt = 0; attempt < 7 && valueKey == null; attempt++ )
		{
			if ( attempt % 3 == 3 )
			{
				Set();
			}

			Console.WriteLine($"Registry key not found " +
			                  $"(attempt {attempt + 1})," +
			                  $" waiting for registry to update");

			await Task.Delay(250, TestContext.CancellationTokenSource.Token);

			valueKey = GetRegistryValue();
			if ( valueKey != null )
			{
				break;
			}
		}

		Assert.IsNotNull(valueKey);
		var match = GetFilePathFromRegistryRegex().Match(valueKey);
		var value = match.Groups[1].Value;

		Assert.AreEqual(filePath, value);
		return;

		void Set()
		{
			WindowsSetFileAssociations.EnsureAssociationsSet(
				new FileAssociation
				{
					Extension = Extension,
					ProgId = ProgId,
					FileTypeDescription = FileTypeDescription,
					ExecutableFilePath = filePath
				});
		}
	}

	[SuppressMessage("Interoperability",
		"CA1416:Validate platform compatibility",
		Justification = "Check if test for windows only")]
	private static string? GetRegistryValue()
	{
		const string registryKeyPath = $@"Software\Classes\{ProgId}\shell\open\command";
		using var key = Registry.CurrentUser.OpenSubKey(registryKeyPath);
		var valueKey = key?.GetValue(string.Empty)?.ToString();
		Console.WriteLine($"GetRegistryValue {valueKey}");
		return valueKey;
	}

	[GeneratedRegex("\"([^\"]*)\"")]
	private static partial Regex GetFilePathFromRegistryRegex();
}
