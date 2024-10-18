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
	[ExcludeFromCoverage]
	public void ArgsHelper_NeedRecruisiveTest()
	{
		var args = new List<string> { "-r" }.ToArray();
		Assert.IsTrue(ArgsHelper.NeedRecursive(args));

		// Bool parse check
		args = new List<string> { "-r", "true" }.ToArray();
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

		// Bool parse cheArgsHelper_GetPath_CurrentDirectory_Testck
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
	public void ArgsHelper_GetPathFormArgsTest_FieldAccessException()
	{
		// Arrange
		var args = new List<string> { "-p", "/" }.ToArray();

		// Act & Assert
		Assert.ThrowsException<FieldAccessException>(() =>
			new ArgsHelper(null!).GetPathFormArgs(args));
	}

	[TestMethod]
	public void GetPathListFormArgsTest_SingleItem()
	{
		var args = new List<string> { "-p", "/" }.ToArray();
		Assert.AreEqual("/",
			new ArgsHelper(_appSettings).GetPathListFormArgs(args).FirstOrDefault());
	}

	[TestMethod]
	public void GetPathListFormArgsTest_MultipleItems()
	{
		var args = new List<string> { "-p", "\"/;/test\"" }.ToArray();
		var result = new ArgsHelper(_appSettings).GetPathListFormArgs(args);

		Assert.AreEqual("/", result.FirstOrDefault());
		Assert.AreEqual("/test", result[1]);
	}

	[TestMethod]
	public void GetPathListFormArgsTest_IgnoreNullOrWhiteSpace()
	{
		var args = new List<string> { "-p", "\"/;\"" }.ToArray();
		var result = new ArgsHelper(_appSettings).GetPathListFormArgs(args);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/", result.FirstOrDefault());
	}

	[TestMethod]
	public void GetPathListFormArgsTest_CurrentDirectory()
	{
		var args = new List<string> { "-p" }.ToArray();
		var result = new ArgsHelper(_appSettings).GetPathListFormArgs(args);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(Directory.GetCurrentDirectory(), result.FirstOrDefault());
	}

	[TestMethod]
	public void GetPathListFormArgsTest_FieldAccessException()
	{
		// Arrange
		var args = new List<string> { "-p", "/" }.ToArray();

		// Act & Assert
		Assert.ThrowsException<FieldAccessException>(() =>
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
	[ExcludeFromCoverage]
	public void ArgsHelper_IfSubPathTest()
	{
		_appSettings.StorageFolder = new CreateAnImage().BasePath;
		var args = new List<string> { "-s", "/" }.ToArray();
		Assert.IsTrue(ArgsHelper.IsSubPathOrPath(args));

		// Default
		args = new List<string> { string.Empty }.ToArray();
		Assert.IsTrue(ArgsHelper.IsSubPathOrPath(args));

		args = new List<string> { "-p", "/" }.ToArray();
		Assert.IsFalse(ArgsHelper.IsSubPathOrPath(args));
	}

	[TestMethod]
	public void ArgsHelper_CurrentDirectory_IfSubpathTest()
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
		var envNameList = new ArgsHelper(_appSettings).EnvNameList.ToArray();

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
		var envNameList = new ArgsHelper(_appSettings).EnvNameList.ToArray();

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
		Assert.ThrowsException<FieldAccessException>(() =>
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
		Assert.ThrowsException<NullReferenceException>(() =>
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
		Assert.IsTrue(console.WrittenLines[0].Contains("Thumbnail"));
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
		Assert.IsTrue(console.WrittenLines[0].Contains("MetaThumbnail"));
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
		Assert.IsTrue(console.WrittenLines[0].Contains("Admin"));
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Geo()
	{
		var console = new FakeConsoleWrapper();
		var geoAppSettings = new AppSettings { ApplicationType = AppSettings.StarskyAppType.Geo };
		new ArgsHelper(geoAppSettings, console)
			.NeedHelpShowDialog();

		Assert.IsNotNull(geoAppSettings);
		Assert.IsTrue(console.WrittenLines[0].Contains("Geo"));
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
		Assert.IsTrue(console.WrittenLines[0].Contains("WebHtml"));
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Importer()
	{
		var console = new FakeConsoleWrapper();
		new ArgsHelper(
				new AppSettings { ApplicationType = AppSettings.StarskyAppType.Importer },
				console)
			.NeedHelpShowDialog();
		Assert.IsTrue(console.WrittenLines[0].Contains("Importer"));
	}

	[TestMethod]
	public void ArgsHelper_NeedHelpShowDialog_Sync()
	{
		var console = new FakeConsoleWrapper();
		new ArgsHelper(new AppSettings { ApplicationType = AppSettings.StarskyAppType.Sync },
				console)
			.NeedHelpShowDialog();
		Assert.IsTrue(console.WrittenLines[0].Contains("Sync"));
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
						new List<AppSettingsPublishProfiles>
						{
							new() { Append = "_append", Copy = true, Folder = "folder" }
						}
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
	public void ArgsHelper_NeedHelpShowDialog_Null_Test()
	{
		Assert.ThrowsException<FieldAccessException>(() =>
			new ArgsHelper(null!).NeedHelpShowDialog());
	}

	[TestMethod]
	public void ArgsHelper_SetEnvironmentToAppSettings_Null_Test()
	{
		Assert.ThrowsException<FieldAccessException>(() =>
			new ArgsHelper(null!).SetEnvironmentToAppSettings());
	}

	[TestMethod]
	public void ArgsHelper_SetEnvironmentToAppSettingsTest()
	{
		var appSettings = new AppSettings();


		var shortNameList = new ArgsHelper(appSettings).ShortNameList.ToArray();
		var envNameList = new ArgsHelper(appSettings).EnvNameList.ToArray();

		var shortTestList = new List<string>();
		for ( var i = 0; i < envNameList.Length; i++ )
		{
			shortTestList.Add(shortNameList[i]);

			if ( envNameList[i] == "app__DatabaseType" )
			{
				shortTestList.Add("InMemoryDatabase"); // need to exact good
				continue;
			}

			if ( envNameList[i] == "app__Structure" )
			{
				shortTestList.Add("/{filename}.ext");
				continue;
			}

			if ( envNameList[i] == "app__DatabaseConnection" )
			{
				shortTestList.Add("test");
				continue;
			}

			if ( envNameList[i] == "app__ExifToolPath" )
			{
				shortTestList.Add("app__ExifToolPath");
				continue;
			}

			if ( envNameList[i] == "app__StorageFolder" )
			{
				shortTestList.Add("app__StorageFolder");
				continue;
			}

			Console.WriteLine(envNameList[i]);

			if ( envNameList[i] == "app__ExifToolImportXmpCreate" )
			{
				shortTestList.Add("true");
				continue;
			}

			// Note:
			// There are values add to the unknown values

			shortTestList.Add(i.ToString());
		}

		// First inject values to evn
		new ArgsHelper(appSettings).SetEnvironmentByArgs(shortTestList);


		// and now read it back
		new ArgsHelper(appSettings).SetEnvironmentToAppSettings();


		Assert.AreEqual("/{filename}.ext", appSettings.Structure);
		Assert.AreEqual(AppSettings.DatabaseTypeList.InMemoryDatabase,
			appSettings.DatabaseType);
		Assert.AreEqual("test", appSettings.DatabaseConnection);
		Assert.AreEqual("app__ExifToolPath", appSettings.ExifToolPath);
		Assert.IsTrue(appSettings.StorageFolder.Contains("app__StorageFolder"));


		// Reset Environment after use
		foreach ( var t in envNameList )
		{
			Environment.SetEnvironmentVariable(t, string.Empty);
		}
	}

	[TestMethod]
	public void ArgsHelper_GetColorClass()
	{
		var args = new List<string> { "--colorclass", "1" }.ToArray();
		var value = ArgsHelper.GetColorClass(args);
		Assert.AreEqual(1, value);
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
}
