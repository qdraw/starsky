using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Models.PublishProfileRemote;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public sealed class AppSettingsTest
{
	private readonly AppSettings _appSettings;

	public AppSettingsTest()
	{
		// Add a dependency injection feature
		var services = new ServiceCollection();
		// Inject Config helper
		services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
		// random config
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
		// inject config as item to a service
		services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
		// build the service
		var serviceProvider = services.BuildServiceProvider();
		// get the service
		_appSettings = serviceProvider.GetRequiredService<AppSettings>();
	}

	[TestMethod]
	public void AppSettingsProviderTest_ReadOnlyFoldersTest()
	{
		_appSettings.ReadOnlyFolders = new List<string> { "test" };
		CollectionAssert.AreEqual(new List<string> { "test" }, _appSettings.ReadOnlyFolders);
	}


	[TestMethod]
	public void AppSettingsProviderTest_SqLiteFullPathTest()
	{
		var dataSource = _appSettings.SqLiteFullPath("Data Source=data.db", string.Empty);
		Assert.Contains("data.db", dataSource);
		Assert.Contains("Data Source=", dataSource);
	}

	[TestMethod]
	public void ReplaceEnvironmentVariable_Nothing()
	{
		var value = AppSettings.ReplaceEnvironmentVariable(string.Empty);
		Assert.AreEqual(string.Empty, value);
	}

	[TestMethod]
	public void ReplaceEnvironmentVariable_SomethingThatShouldBeIgnored()
	{
		var value = AppSettings.ReplaceEnvironmentVariable("/test");
		Assert.AreEqual("/test", value);
	}

	[TestMethod]
	public void ReplaceEnvironmentVariable_Non_Existing_EnvVariable()
	{
		var value = AppSettings.ReplaceEnvironmentVariable("$test12345");
		Assert.AreEqual("$test12345", value);
	}

	[TestMethod]
	public void ReplaceEnvironmentVariable_Existing_EnvVariable()
	{
		Environment.SetEnvironmentVariable("test123456789", "123456789");
		// should start with a dollar sign
		var value = AppSettings.ReplaceEnvironmentVariable("$test123456789");
		Assert.AreEqual("123456789", value);
	}

	[TestMethod]
	public void AppSettingsProviderTest_SqLiteFullPathStarskyCliTest()
	{
		_appSettings.DatabaseType = AppSettings.DatabaseTypeList.Sqlite;

		var datasource = _appSettings.SqLiteFullPath(
			"Data Source=data.db", Path.DirectorySeparatorChar + "starsky");
		Assert.Contains("data.db", datasource);
		Assert.Contains("Data Source=", datasource);
	}

	[TestMethod]
	public void AppSettingsProviderTest_SQLite_ExpectException()
	{
		_appSettings.DatabaseType = AppSettings.DatabaseTypeList.Sqlite;

		Assert.ThrowsExactly<ArgumentException>(() =>
			_appSettings.SqLiteFullPath(string.Empty, null!)
		);
		// Optionally, you can assert specific properties of the exception here, but it's not required.
	}

	[TestMethod]
	public void AppSettingsProviderTest_MySQL_ExpectException()
	{
		_appSettings.DatabaseType = AppSettings.DatabaseTypeList.Mysql;

		Assert.ThrowsExactly<ArgumentException>(() =>
			_appSettings.SqLiteFullPath(string.Empty, null!)
		);
		// Optionally, you can assert specific properties of the exception here, but it's not required.
	}

	[TestMethod]
	public void AppSettingsProviderTest_StructureFails_withoutExtAndNoSlash()
	{
		var result = new AppSettingsStructureModel("\\d");
		Assert.AreEqual("Structure '/\\d' is not valid", result.Errors.LastOrDefault());
	}

	[TestMethod]
	public void AppSettingsProviderTest_Structure_withExtAndNoSlash()
	{
		var result = new AppSettingsStructureModel("\\d.ext");

		Assert.IsEmpty(result.Errors);
		Assert.AreEqual("/\\d.ext", result.DefaultPattern);
	}

	[TestMethod]
	public void AppSettingsNameNullTest()
	{
		var appSettings = new AppSettings { Name = null! };
		Assert.AreEqual(string.Empty, appSettings.Name);
	}

	[TestMethod]
	public void AppSettingsNameNoWebSafeNameTest()
	{
		Assert.AreEqual("non-websafe-name/",
			new AppSettings().GetWebSafeReplacedName("{name}", "Non websafe name"));
	}

	[TestMethod]
	public void AppSettingsCameraTimeZoneStringEmpty()
	{
		var appSettings = new AppSettings();
		Assert.AreEqual(string.Empty, appSettings.CameraTimeZone);
	}

	[TestMethod]
	public void AppSettingsCameraTimeZoneExample()
	{
		var appSettings = new AppSettings
		{
			CameraTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam")
		};

		Console.WriteLine(appSettings.CameraTimeZone);

		// Linux: Europe/Amsterdam
		// Windows: W. Europe Standard Time

		Assert.AreEqual("Europe/Amsterdam", appSettings.CameraTimeZone);
	}

	[TestMethod]
	public void ConvertTimeZoneId_ForNonWindows_IanaId__UnixOnly()
	{
		if ( _appSettings.IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		var value = AppSettings.ConvertTimeZoneId("Europe/Berlin");

		// Linux: Europe/Amsterdam
		// Windows: W. Europe Standard Time

		Assert.AreEqual("Europe/Berlin", value.Id);
	}

	[TestMethod]
	public void ConvertTimeZoneId_WindowsId__WindowsOnly()
	{
		if ( !_appSettings.IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var value = AppSettings.ConvertTimeZoneId("Europe/Berlin");

		// Linux: Europe/Amsterdam
		// Windows: W. Europe Standard Time

		Assert.AreEqual("W. Europe Standard Time", value.Id);
	}

	[TestMethod]
	public void ConvertTimeZoneId_Antarctica_IanaId__UnixOnly()
	{
		var value = AppSettings.ConvertTimeZoneId("Antarctica/Troll");

		// Linux: Antarctica/Troll
		// Windows: In older versions it does not exist

		Assert.AreEqual("Antarctica/Troll", value.Id);
	}

	[TestMethod]
	public void SyncServiceRenameListItemsToDbStyleTest()
	{
		var appSettings = new AppSettings();

		var newImage = new CreateAnImage();
		_appSettings.StorageFolder = newImage.BasePath; // needs to have an / or \ at the end
		var inputList = new List<string> { Path.DirectorySeparatorChar.ToString() };
		var expectedOutputList = new List<string> { "/" };
		var output = appSettings.RenameListItemsToDbStyle(inputList);
		// list of files names that are starting with a filename (and not an / or \)

		CollectionAssert.AreEqual(expectedOutputList, output);
	}

	[TestMethod]
	public void AppSettings_CloneToDisplay_hideSecurityItems()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Default =
				[
					new RemoteCredentialWrapper
					{
						Ftp = new FtpCredential { WebFtp = "ftp://t:t@m.com" }
					}
				],
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
				{
					{
						"test", [
							new RemoteCredentialWrapper
							{
								Ftp = new FtpCredential { WebFtp = "ftp://t:t@m.com" }
							}
						]
					}
				}
			}
		};

		var display = appSettings.CloneToDisplay();
		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.DatabaseConnection);

		Assert.Contains(AppSettings.CloneToDisplaySecurityWarning,
			display.PublishProfilesRemote.Default[0].Ftp?.WebFtp ?? string.Empty);
		Assert.Contains(AppSettings.CloneToDisplaySecurityWarning,
			display.PublishProfilesRemote.Profiles["test"][0].Ftp?.WebFtp ?? string.Empty);
	}

	[TestMethod]
	public void AppSettings_CloneToDisplay_hideSecurityItems2()
	{
		var appSettings = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				Header = "test",
				LogsHeader = "test",
				MetricsHeader = "test",
				TracesHeader = "test"
			}
		};

		var display = appSettings.CloneToDisplay();

		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.OpenTelemetry?.Header);
		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.OpenTelemetry?.LogsHeader);
		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.OpenTelemetry?.MetricsHeader);
		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.OpenTelemetry?.TracesHeader);
	}


	[TestMethod]
	public void AppSettings_CloneToDisplay_skip_Null_SecurityItems2_OpenTelemetrySettings()
	{
		var appSettings = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				Header = null, LogsHeader = null, MetricsHeader = null, TracesHeader = null
			}
		};

		var display = appSettings.CloneToDisplay();

		Assert.IsNull(display.OpenTelemetry?.Header);
		Assert.IsNull(display.OpenTelemetry?.LogsHeader);
		Assert.IsNull(display.OpenTelemetry?.MetricsHeader);
		Assert.IsNull(display.OpenTelemetry?.TracesHeader);
	}

	[TestMethod]
	public void
		AppSettings_CloneToDisplay_skip_object_Null_SecurityItems2_OpenTelemetrySettings()
	{
		var appSettings = new AppSettings { OpenTelemetry = null };

		var display = appSettings.CloneToDisplay();

		Assert.IsNull(display.OpenTelemetry);
	}

	[TestMethod]
	public void AppSettings_CloneToDisplay_hideSecurityItems_PublishProfiles()
	{
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test",
					new List<AppSettingsPublishProfiles>
					{
						new() { Copy = true }, new() { Path = "value", Prepend = "value" }
					}
				}
			}
		};

		var display = appSettings.CloneToDisplay();
		// nr 0
		Assert.AreEqual(string.Empty, display.PublishProfiles?["test"][0].Path);
		Assert.AreEqual(string.Empty, display.PublishProfiles?["test"][0].Prepend);
		// nr 1
		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.PublishProfiles?["test"][1].Path);
		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.PublishProfiles?["test"][1].Prepend);
	}

	[TestMethod]
	public void AppSettings_CloneToDisplay_CloudImport()
	{
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Credentials = new CloudProviderCredentials { RefreshToken = "test" }
					},
					new CloudImportProviderSettings
					{
						Credentials = new CloudProviderCredentials { RefreshToken = "test1" }
					}
				]
			}
		};

		var display = appSettings.CloneToDisplay();

		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.CloudImport?.Providers[0].Credentials.RefreshToken);
		Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,
			display.CloudImport?.Providers[1].Credentials.RefreshToken);
	}

	[TestMethod]
	public void AppSettings_CloneToDisplay_CloudImport_Null()
	{
		var appSettings = new AppSettings { CloudImport = null };

		var display = appSettings.CloneToDisplay();

		Assert.IsNull(display.CloudImport);
	}

	[TestMethod]
	public void AppSettings_CloneToDisplay_CloudImport_Null2()
	{
		var appSettings =
			new AppSettings { CloudImport = new CloudImportSettings { Providers = null! } };

		var display = appSettings.CloneToDisplay();

		Assert.IsNull(display.CloudImport?.Providers);
	}

	[TestMethod]
	public void AppSettings_IsReadOnly_NullNoItemTest()
	{
		var appSettings = new AppSettings();
		Assert.IsFalse(appSettings.IsReadOnly(string.Empty));
	}

	[TestMethod]
	public void PublishProfiles_Null()
	{
		var appSettings = new AppSettings { PublishProfiles = null };
		Assert.AreEqual(0, appSettings.PublishProfiles?.Count);
	}

	[TestMethod]
	public void AppVersionBuildDateTime()
	{
		var appVersionBuildDateTime = new AppSettings().AppVersionBuildDateTime;
		Assert.IsGreaterThan(2000, appVersionBuildDateTime.Year);
	}

	[TestMethod]
	public void AppVersion()
	{
		var appVersionBuildDateTime = new AppSettings().AppVersion;
		Assert.IsNotNull(appVersionBuildDateTime);
	}

	[TestMethod]
	public void EnablePackageTelemetry_True()
	{
		var appSettings = new AppSettings { EnablePackageTelemetry = true };
		Assert.IsTrue(appSettings.EnablePackageTelemetry);
	}

	[TestMethod]
	public void EnablePackageTelemetry_False()
	{
		var appSettings = new AppSettings { EnablePackageTelemetry = false };
		Assert.IsFalse(appSettings.EnablePackageTelemetry);
	}

#if(DEBUG)
	[TestMethod]
	public void EnablePackageTelemetry_Debug_False()
	{
		var appSettings = new AppSettings();
		Assert.IsFalse(appSettings.EnablePackageTelemetry);
	}
#else
		[TestMethod]
		public void EnablePackageTelemetry_Release_True()
		{
			var appSettings = new AppSettings();
			Assert.IsTrue(appSettings.EnablePackageTelemetry);
		}
#endif

	[TestMethod]
	public void AccountRolesByEmailRegisterOverwrite_BogusShouldBeIgnored()
	{
		var appSettings = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite =
				new Dictionary<string, string> { { "bogusEmail", "bogusRole" } }
		};
		Assert.IsEmpty(appSettings.AccountRolesByEmailRegisterOverwrite);
	}

	[TestMethod]
	public void AccountRolesByEmailRegisterOverwrite_Null()
	{
		var appSettings = new AppSettings { AccountRolesByEmailRegisterOverwrite = null };
		Assert.AreEqual(0, appSettings.AccountRolesByEmailRegisterOverwrite?.Count);
	}

	[TestMethod]
	public void AccountRolesByEmailRegisterOverwrite_ValidRole()
	{
		var appSettings = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite =
				new Dictionary<string, string> { { "bogusEmail", "Administrator" } }
		};

		Assert.HasCount(1, appSettings.AccountRolesByEmailRegisterOverwrite);
		Assert.AreEqual("Administrator",
			appSettings.AccountRolesByEmailRegisterOverwrite["bogusEmail"]);
	}

	[TestMethod]
	public void AccountRolesByEmailRegisterOverwrite_ValidRole2()
	{
		var appSettings = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite =
				new Dictionary<string, string> { { "bogusEmail", "Administrator" } }
		};
		Assert.HasCount(1, appSettings.AccountRolesByEmailRegisterOverwrite);

		appSettings.AccountRolesByEmailRegisterOverwrite.Add("bogusEmail2", "Administrator");

		Assert.HasCount(2, appSettings.AccountRolesByEmailRegisterOverwrite);
		Assert.AreEqual("Administrator",
			appSettings.AccountRolesByEmailRegisterOverwrite["bogusEmail2"]);
	}

	[TestMethod]
	public void DatabasePathToFilePath_NoNull()
	{
		var appSettings = new AppSettings();

		var result = appSettings.DatabasePathToFilePath("\\test");

		Assert.IsNotNull(result);
	}

	[TestMethod]
	public void AppSettingsToTransferObjectConversion()
	{
		// Arrange
		var appSettings = new AppSettings { StorageFolder = "Value1", Verbose = true };

		// Act
		var transferObject = ( AppSettingsTransferObject ) appSettings;

		// Assert
		Assert.AreEqual(appSettings.StorageFolder, transferObject.StorageFolder);
		Assert.AreEqual(appSettings.Verbose, transferObject.Verbose);
	}

	[TestMethod]
	public void TransferObjectToAppSettingsConversion()
	{
		// Arrange
		var transferObject = new AppSettingsTransferObject
		{
			StorageFolder = $"Value1{Path.DirectorySeparatorChar}", Verbose = true
		};

		// Act
		var appSettings = ( AppSettings ) transferObject;

		// Assert
		Assert.AreEqual(appSettings.StorageFolder, transferObject.StorageFolder);
		Assert.AreEqual(appSettings.Verbose, transferObject.Verbose);
	}

	[TestMethod]
	public void CopyProperties_CopiesPropertiesCorrectly()
	{
		// Arrange
		var source = new AppSettings { StorageFolder = "Value1", Verbose = true };
		var destination = new AppSettings();

		// Act
		AppSettings.CopyProperties(source, destination);

		// Assert
		Assert.AreEqual(source.StorageFolder, destination.StorageFolder);
		Assert.AreEqual(source.Verbose, destination.Verbose);
	}

	[TestMethod]
	public void AppSettings_AppSettingsLocalPath_Null()
	{
		var appSettings = new AppSettings { AppSettingsLocalPath = null! };
		Assert.AreEqual(string.Empty, appSettings.AppSettingsLocalPath);
	}

	[TestMethod]
	public void AppSettings_AppSettingsLocalPath_AssemblyDirectory()
	{
		var appSettings = new AppSettings { AppSettingsLocalPath = "/test/{AssemblyDirectory}" };

		var expectedResult = $"/test/{appSettings.BaseDirectoryProject}";

		Assert.AreEqual(expectedResult, appSettings.AppSettingsLocalPath);
	}
}
