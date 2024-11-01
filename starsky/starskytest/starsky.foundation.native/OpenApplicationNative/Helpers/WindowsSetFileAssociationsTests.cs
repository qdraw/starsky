using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

/// <summary>
/// Only for Windows - test the WindowsSetFileAssociationsWindows
/// </summary>
[TestClass]
public partial class WindowsSetFileAssociationsTests
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
		try
		{
			Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{Extension}", false);
			Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{ProgId}", false);
		}
		catch ( IOException )
		{
			// Ignore if the key does not exist
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
		WindowsSetFileAssociations.EnsureAssociationsSet(
			new FileAssociation
			{
				Extension = Extension,
				ProgId = ProgId,
				FileTypeDescription = FileTypeDescription,
				ExecutableFilePath = filePath
			});

		var valueKey = GetRegistryValue();

		if ( valueKey == null )
		{
			Console.WriteLine("Registry key not found, waiting for registry to update");
			await Task.Delay(1000); // Wait for the registry to update
			valueKey = GetRegistryValue();
		}

		Assert.IsNotNull(valueKey);
		var match = GetFilePathFromRegistryRegex().Match(valueKey);
		var value = match.Groups[1].Value;

		Assert.AreEqual(filePath, value);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
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
