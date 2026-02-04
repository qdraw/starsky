using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.project.web.Attributes;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class ArgsHelperTest
{
	private readonly AppSettings _appSettings;

	public ArgsHelperTest()
	{
		// Add a dependency injection feature
		var services = new ServiceCollection();
		// Inject Config helper
		services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
		// Make example config in memory
		var newImage = new CreateAnImage();
		var dict = new Dictionary<string, string?>
		{
			{ "App:StorageFolder", newImage.BasePath }, { "App:Verbose", "true" }
		};
		// Start using dependency injection
		var builder = new ConfigurationBuilder();
		// Add random config to dependency injection
		builder.AddInMemoryCollection(dict);
		// build config
		var configuration = builder.Build();
		// inject config as object to a service
		services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
		// build the service
		var serviceProvider = services.BuildServiceProvider();
		// get the service
		_appSettings = serviceProvider.GetRequiredService<AppSettings>();
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_NeedVerboseTest()
	{
		var args = new List<string> { "-v" }.ToArray();
		Assert.IsTrue(ArgsHelper.NeedVerbose(args));

		// Bool parse check
		args = new List<string> { "-v", "true" }.ToArray();
		Assert.IsTrue(ArgsHelper.NeedVerbose(args));
	}

	[TestMethod]
	[DataRow("-r", "true")]
	[DataRow("-r", null)]
	[DataRow("--recursive", "true")]
	[DataRow("--recursive", null)]
	public void ArgsHelper_NeedRecursiveTest(string arg1, string? arg2)
	{
		string[] args;
		if ( arg2 == null )
		{
			args = [arg1];
		}
		else
		{
			args = [arg1, arg2];
		}

		Assert.IsTrue(ArgsHelper.NeedRecursive(args));
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_NeedCacheCleanupTest()
	{
		var args = new List<string> { "-x" }.ToArray();
		Assert.IsTrue(ArgsHelper.NeedCleanup(args));

		// Bool parse check
		args = new List<string> { "-x", "true" }.ToArray();
		Assert.IsTrue(ArgsHelper.NeedCleanup(args));
	}


	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_GetIndexModeTest()
	{
		// Default on so testing off
		var args = new List<string> { "-i", "false" }.ToArray();
		Assert.IsFalse(ArgsHelper.GetIndexMode(args));
	}


	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_NeedHelpTest()
	{
		var args = new List<string> { "-h" }.ToArray();
		Assert.IsTrue(ArgsHelper.NeedHelp(args));

		// Bool parse cheArgsHelper_GetPath_CurrentDirectory_Test
		args = new List<string> { "-h", "true" }.ToArray();
		Assert.IsTrue(ArgsHelper.NeedHelp(args));
	}

	[TestMethod]
	public void ArgsHelper_GetPathFormArgsTest()
	{
		var args = new List<string> { "-p", "/" }.ToArray();
		Assert.AreEqual("/", new ArgsHelper(_appSettings).GetPathFormArgs(args));
	}

	[TestMethod]
	[DataRow("-p", "test", "/test")]
	[DataRow("--path", "test", "/test")]
	public void ArgsHelper_GetPathFormArgsDataTest(string arg1, string arg2, string expected)
	{
		string[] args = [arg1, arg2];
		var result = new ArgsHelper(_appSettings).GetPathFormArgs(args);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void GetUserInputPassword()
	{
		var args = new List<string> { "-p", "test" }.ToArray();
		Assert.AreEqual("test", ArgsHelper.GetUserInputPassword(args));
	}

	[TestMethod]
	public void GetUserInputPasswordLong()
	{
		var args = new List<string> { "--password", "test" }.ToArray();
		Assert.AreEqual("test", ArgsHelper.GetUserInputPassword(args));
	}

	[TestMethod]
	public void GetPathFormArgs_FieldAccessException()
	{
		// Arrange
		var args = new List<string> { "-p", "/" }.ToArray();

		// Act & Assert
		Assert.ThrowsExactly<FieldAccessException>(() =>
			new ArgsHelper(null!).GetPathFormArgs(args));
	}

	[TestMethod]
	public void GetPathListFormArgs_SingleItem()
	{
		var args = new List<string> { "-p", "/" }.ToArray();
		Assert.AreEqual("/",
			new ArgsHelper(_appSettings).GetPathListFormArgs(args).FirstOrDefault());
	}

	[TestMethod]
	public void GetPathListFormArgs_MultipleItems()
	{
		var args = new List<string> { "-p", "\"/;/test\"" }.ToArray();
		var result = new ArgsHelper(_appSettings).GetPathListFormArgs(args);

		Assert.AreEqual("/", result.FirstOrDefault());
		Assert.AreEqual("/test", result[1]);
	}

	[TestMethod]
	public void GetPathListFormArgs_IgnoreNullOrWhiteSpace()
	{
		var args = new List<string> { "-p", "\"/;\"" }.ToArray();
		var result = new ArgsHelper(_appSettings).GetPathListFormArgs(args);

		Assert.HasCount(1, result);
		Assert.AreEqual("/", result.FirstOrDefault());
	}

	[TestMethod]
	public void GetPathListFormArgs_CurrentDirectory()
	{
		var args = new List<string> { "-p" }.ToArray();
		var result = new ArgsHelper(_appSettings).GetPathListFormArgs(args);

		Assert.HasCount(1, result);
		Assert.AreEqual(Directory.GetCurrentDirectory(), result.FirstOrDefault());
	}

	[TestMethod]
	public void GetPathListFormArgs_PathStartsWithDash_CurrentDirectoryReturned()
	{
		// Arrange
		var argsHelper = new ArgsHelper(new AppSettings());
		var args = new List<string> { "-p", "-otherarg" };

		// Act
		var result = argsHelper.GetPathListFormArgs(args);

		// Assert
		Assert.AreEqual(Directory.GetCurrentDirectory(), result[0]);
	}

	[TestMethod]
	public void GetPathListFormArgsTest_FieldAccessException()
	{
		// Arrange
		var args = new List<string> { "-p", "/" }.ToArray();

		// Act & Assert
		Assert.ThrowsExactly<FieldAccessException>(() =>
			new ArgsHelper(null!).GetPathListFormArgs(args));
	}

	[TestMethod]
	public void ArgsHelper_GetPath_WithHelp_CurrentDirectory_Test()
	{
		var args = new List<string> { "-p", "-h" }.ToArray();
		var value = new ArgsHelper(_appSettings).GetPathFormArgs(args, false);

		var currentDir = Directory.GetCurrentDirectory();
		Assert.AreEqual(currentDir, value);
	}

	[TestMethod]
	public void ArgsHelper_GetPath_CurrentDirectory_Test()
	{
		var args = new List<string> { "-p" }.ToArray();
		var value = new ArgsHelper(_appSettings).GetPathFormArgs(args, false);

		Assert.AreEqual(Directory.GetCurrentDirectory(), value);
	}


	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_GetSubpathFormArgsTest()
	{
		_appSettings.StorageFolder = new CreateAnImage().BasePath;
		var args = new List<string> { "-s", "/" }.ToArray();
		Assert.AreEqual("/", ArgsHelper.GetSubPathFormArgs(args));
	}

	[TestMethod]
	public void ArgsHelper_IfSubPathTest1()
	{
		var args = new List<string> { "-s", "/" }.ToArray();
		Assert.IsTrue(ArgsHelper.IsSubPathOrPath(args));
	}

	[TestMethod]
	public void ArgsHelper_IfSubPathTest2()
	{
		// Default
		var args = new List<string> { string.Empty }.ToArray();
		Assert.IsTrue(ArgsHelper.IsSubPathOrPath(args));
	}

	[TestMethod]
	public void ArgsHelper_IfSubPathTest3()
	{
		var args = new List<string> { "-p", "/" }.ToArray();
		Assert.IsFalse(ArgsHelper.IsSubPathOrPath(args));
	}

	[TestMethod]
	public void ArgsHelper_IfSubPathTest4()
	{
		var args = new List<string> { "--path", "/" }.ToArray();
		Assert.IsFalse(ArgsHelper.IsSubPathOrPath(args));
	}

	[TestMethod]
	public void ArgsHelper_CurrentDirectory_IfSubPathTest()
	{
		// for selecting the current directory
		var args = new List<string> { "-p" }.ToArray();
		Assert.IsFalse(ArgsHelper.IsSubPathOrPath(args));
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_GetThumbnailTest()
	{
		_appSettings.StorageFolder = new CreateAnImage().BasePath;
		var args = new List<string> { "-t", "true" }.ToArray();
		Assert.IsTrue(ArgsHelper.GetThumbnail(args));
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_GetOrphanFolderCheckTest()
	{
		_appSettings.StorageFolder = new CreateAnImage().BasePath;
		var args = new List<string> { "-o", "true" }.ToArray();
		Assert.IsTrue(new ArgsHelper(_appSettings).GetOrphanFolderCheck(args));
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_GetMoveTest()
	{
		var args = new List<string> { "-m" }.ToArray();
		Assert.IsTrue(ArgsHelper.GetMove(args));

		// Bool parse check
		args = new List<string> { "-m", "true" }.ToArray();
		Assert.IsTrue(ArgsHelper.GetMove(args));
	}

	[TestMethod]
	public void ArgsHelper_GetMoveTest2()
	{
		// Bool parse check
		var args = new List<string> { "-m", "false" }.ToArray();
		Assert.IsFalse(ArgsHelper.GetMove(args));
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_GetAllTest()
	{
		var args = new List<string> { "-a" }.ToArray();
		Assert.IsTrue(ArgsHelper.GetAll(args));

		// Bool parse check
		args = new List<string> { "-a", "false" }.ToArray();
		Assert.IsFalse(ArgsHelper.GetAll(args));

		args = new List<string>().ToArray();
		Assert.IsFalse(ArgsHelper.GetAll(args));
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_SetEnvironmentByArgsShortTestListTest()
	{
		var shortNameList = new ArgsHelper(_appSettings).ShortNameList.ToArray();
		var envNameList = ArgsHelper.EnvNameList.ToArray();

		var shortTestList = new List<string>();
		for ( var i = 0; i < shortNameList.Length; i++ )
		{
			shortTestList.Add(shortNameList[i]);
			shortTestList.Add(i.ToString());
		}

		new ArgsHelper(_appSettings).SetEnvironmentByArgs(shortTestList);

		for ( var i = 0; i < envNameList.Length; i++ )
		{
			Assert.AreEqual(Environment.GetEnvironmentVariable(envNameList[i]), i.ToString());
		}

		// Reset Environment after use
		foreach ( var t in envNameList )
		{
			Environment.SetEnvironmentVariable(t, string.Empty);
		}
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public void ArgsHelper_SetEnvironmentByArgsLongTestListTest()
	{
		var longNameList = new ArgsHelper(_appSettings).LongNameList.ToArray();
		var envNameList = ArgsHelper.EnvNameList.ToArray();

		var longTestList = new List<string>();
		for ( var i = 0; i < longNameList.Length; i++ )
		{
			longTestList.Add(longNameList[i]);
			longTestList.Add(i.ToString());
		}

		new ArgsHelper(_appSettings).SetEnvironmentByArgs(longTestList);

		for ( var i = 0; i < envNameList.Length; i++ )
		{
			Assert.AreEqual(Environment.GetEnvironmentVariable(envNameList[i]), i.ToString());
		}

		// Reset Environment after use
		foreach ( var t in envNameList )
		{
			Environment.SetEnvironmentVariable(t, string.Empty);
		}
	}

	[TestMethod]
	public void ArgsHelper_GetRelativeValue_Null_Test()
	{
		var args = new List<string> { "--subpathrelative", "1" }.ToArray();
		Assert.ThrowsExactly<FieldAccessException>(() =>
			new ArgsHelper(null!).GetRelativeValue(args));
	}

	[TestMethod]
	public void ArgsHelper_GetSubPathRelativeTest()
	{
		var args = new List<string> { "--subpathrelative", "1" }.ToArray();
		var relative = new ArgsHelper(_appSettings).GetRelativeValue(args);
		Assert.AreEqual(-1, relative);
	}

	[TestMethod]
	public void ArgsHelper_GetSubPathRelativeTestMinusValue()
	{
		var args = new List<string> { "--subpathrelative", "-1" }.ToArray();
		var relative = new ArgsHelper(_appSettings).GetRelativeValue(args);
		Assert.AreEqual(-1, relative);
	}

	[TestMethod]
	public void ArgsHelper_GetSubPathRelative_Null_Test()
	{
		// Arrange
		ArgsHelper? argsHelper = null;

		// Act & Assert
		Assert.ThrowsExactly<NullReferenceException>(() =>
		{
			argsHelper!.GetRelativeValue(new List<string>());
		});
	}

	[TestMethod]
	public void ArgsHelper_GetSubPathRelativeTestLargeInt()
	{
		var args = new List<string> { "--subpathrelative", "201801020" }.ToArray();
		var relative = new ArgsHelper(_appSettings).GetRelativeValue(args);
		Assert.IsNull(relative);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Thumbnail()
	{
		var console = new FakeConsoleWrapper();
		// Just simple show a console dialog
		new ArgsHelper(
				new AppSettings
				{
					ApplicationType = AppSettings.StarskyAppType.Thumbnail, Verbose = true
				}, console)
			.NeedHelpShowDialog();
		Assert.Contains("Thumbnail", console.WrittenLines[0]);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_MetaThumbnail()
	{
		var console = new FakeConsoleWrapper();
		// Just simple show a console dialog
		new ArgsHelper(
				new AppSettings
				{
					ApplicationType = AppSettings.StarskyAppType.MetaThumbnail, Verbose = true
				}, console)
			.NeedHelpShowDialog();
		Assert.Contains("MetaThumbnail", console.WrittenLines[0]);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Admin()
	{
		var console = new FakeConsoleWrapper();
		new ArgsHelper(
				new AppSettings
				{
					ApplicationType = AppSettings.StarskyAppType.Admin, Verbose = true
				}, console)
			.NeedHelpShowDialog();
		Assert.Contains("Admin", console.WrittenLines[0]);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Geo()
	{
		var console = new FakeConsoleWrapper();
		var geoAppSettings = new AppSettings { ApplicationType = AppSettings.StarskyAppType.Geo };
		new ArgsHelper(geoAppSettings, console)
			.NeedHelpShowDialog();

		Assert.IsNotNull(geoAppSettings);
		Assert.Contains("Geo", console.WrittenLines[0]);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_WebHtml()
	{
		var console = new FakeConsoleWrapper();
		new ArgsHelper(
				new AppSettings
				{
					ApplicationType = AppSettings.StarskyAppType.WebHtml, Verbose = true
				}, console)
			.NeedHelpShowDialog();
		Assert.Contains("WebHtml", console.WrittenLines[0]);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Importer()
	{
		var console = new FakeConsoleWrapper();
		new ArgsHelper(
				new AppSettings { ApplicationType = AppSettings.StarskyAppType.Importer },
				console)
			.NeedHelpShowDialog();
		Assert.Contains("Importer", console.WrittenLines[0]);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Sync()
	{
		var console = new FakeConsoleWrapper();
		new ArgsHelper(new AppSettings { ApplicationType = AppSettings.StarskyAppType.Sync },
				console)
			.NeedHelpShowDialog();
		Assert.Contains("Sync", console.WrittenLines[0]);
	}

	[TestMethod]
	public void NeedHelpShowDialog_WebHtml_Verbose()
	{
		var consoleWrapper = new FakeConsoleWrapper();
		var appSettings =
			new AppSettings
			{
				Verbose = true,
				ApplicationType = AppSettings.StarskyAppType.WebHtml,
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{
						"_d",
						[
							new AppSettingsPublishProfiles
							{
								Append = "_append", Copy = true, Folder = "folder"
							}
						]
					}
				}
			};

		new ArgsHelper(appSettings, consoleWrapper)
			.NeedHelpShowDialog();

		var contains = consoleWrapper.WrittenLines.Contains(
			"--- Path:  Append: _append Copy: True Folder: folder/ Prepend:  Template:  " +
			"ContentType: None MetaData: True OverlayMaxWidth: 100 SourceMaxWidth: 100 ");

		Assert.IsTrue(contains);
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_OpenTelemetry()
	{
		var console = new FakeConsoleWrapper();
		var appSettings = new AppSettings
		{
			ApplicationType = AppSettings.StarskyAppType.Sync,
			OpenTelemetry = new OpenTelemetrySettings
			{
				LogsEndpoint = "http://localhost:4317",
				TracesEndpoint = "http://localhost:4318",
				MetricsEndpoint = "http://localhost:4319"
			},
			Verbose = true
		};
		new ArgsHelper(appSettings, console).NeedHelpShowDialog();

		Assert.IsTrue(console.WrittenLines.Exists(p => p.Contains($"OpenTelemetry LogsEndpoint: " +
			$"{appSettings.OpenTelemetry.LogsEndpoint}")));
		Assert.IsTrue(console.WrittenLines.Exists(p =>
			p.Contains($"OpenTelemetry TracesEndpoint: " +
			           $"{appSettings.OpenTelemetry.TracesEndpoint}")));
		Assert.IsTrue(console.WrittenLines.Exists(p =>
			p.Contains($"OpenTelemetry MetricsEndpoint: " +
			           $"{appSettings.OpenTelemetry.MetricsEndpoint}")));
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_OpenTelemetry_Null()
	{
		var console = new FakeConsoleWrapper();
		var appSettings = new AppSettings
		{
			ApplicationType = AppSettings.StarskyAppType.Sync,
			OpenTelemetry = null, // on purpose null
			Verbose = true
		};
		new ArgsHelper(appSettings, console).NeedHelpShowDialog();

		// does not contain OpenTelemetry, because it is null
		Assert.IsFalse(console.WrittenLines.Exists(p => p.Contains("OpenTelemetry")));
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Null_Test()
	{
		Assert.ThrowsExactly<FieldAccessException>(() =>
			new ArgsHelper(null!).NeedHelpShowDialog());
	}

	[TestMethod]
	public void ArgsHelper_GetColorClass()
	{
		var args = new List<string> { "--colorclass", "1" }.ToArray();
		var value = ArgsHelper.GetColorClass(args);
		Assert.AreEqual(1, value);
	}

	[TestMethod]
	public void ArgsHelper_GetOrigin()
	{
		var args = new List<string> { "--origin", "1" }.ToArray();
		var value = ArgsHelper.GetOrigin(args);
		Assert.AreEqual("1", value);
	}

	[TestMethod]
	public void ArgsHelper_GetOrigin_Fallback()
	{
		var args = new List<string> { "--origin" }.ToArray();
		var value = ArgsHelper.GetOrigin(args);
		Assert.AreEqual(string.Empty, value);
	}

	[TestMethod]
	public void ArgsHelper_GetColorClass_99_Fallback()
	{
		var args = new List<string> { "--colorclass", "99" }.ToArray();
		var value = ArgsHelper.GetColorClass(args);
		Assert.AreEqual(-1, value);
	}

	[TestMethod]
	public void ArgsHelper_GetColorClassFallback()
	{
		var args = new List<string>().ToArray();
		var value = ArgsHelper.GetColorClass(args);
		Assert.AreEqual(-1, value);
	}

	[TestMethod]
	public void Name()
	{
		_appSettings.StorageFolder = new CreateAnImage().BasePath;
		var args = new List<string> { "-n", "test" }.ToArray();
		Assert.AreEqual("test", ArgsHelper.GetName(args));
	}

	[TestMethod]
	public void ArgsHelper_GetProfile()
	{
		var args = new List<string> { "--profile", "test" }.ToArray();
		var value = ArgsHelper.GetProfile(args);
		Assert.AreEqual("test", value);
	}

	[TestMethod]
	public void ArgsHelper_GetProfile_StringEmpty()
	{
		var args = new List<string> { "--profile" }.ToArray();
		var value = ArgsHelper.GetProfile(args);
		Assert.AreEqual(string.Empty, value);
	}

	[TestMethod]
	[DataRow("--runtime osx-arm64,osx-x64", "osx-arm64,osx-x64")]
	[DataRow("--runtime win-x64", "win-x64")]
	[DataRow("--runtime win-x64-linux-x64", "win-x64-linux-x64")]
	[DataRow("--other value", "")]
	public void GetRuntimes_ShouldReturnExpectedRuntimes(string args, string expected)
	{
		// Act
		var result = ArgsHelper.GetRuntimes([.. args.Split(' ')]);

		// Assert
		Assert.AreEqual(expected, string.Join(",", result));
	}

	[TestMethod]
	public void GetRuntimes_QuotedInFirstArgResults()
	{
		const string expected = "osx-arm64,osx-x64";
		var result = ArgsHelper.GetRuntimes(["--runtime osx-arm64,osx-x64"]);
		Assert.AreEqual(expected, string.Join(",", result));
	}
}
