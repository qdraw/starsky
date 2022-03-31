using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using TimeZoneConverter;

namespace starskytest.starsky.foundation.platform.Models
{
	[TestClass]
	public class AppSettingsTest
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
			var dict = new Dictionary<string, string>
			{
				{"App:StorageFolder", newImage.BasePath},
				{"App:Verbose", "true"}
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
		public void AppSettingsProviderTest_ReadOnlyFoldersTest()
		{
			_appSettings.ReadOnlyFolders = new List<string> {"test"};
			CollectionAssert.AreEqual(new List<string> {"test"}, _appSettings.ReadOnlyFolders);
		}


		[TestMethod]
		public void AppSettingsProviderTest_SqLiteFullPathTest()
		{
			var dataSource = _appSettings.SqLiteFullPath("Data Source=data.db", string.Empty);
			Assert.AreEqual(true, dataSource.Contains("data.db") );
			Assert.AreEqual(true, dataSource.Contains("Data Source="));
		}
		
		[TestMethod]
		public void ReplaceEnvironmentVariable_Nothing()
		{
			var value = _appSettings.ReplaceEnvironmentVariable(string.Empty);
			Assert.AreEqual(string.Empty, value);
		}

		[TestMethod]
		public void ReplaceEnvironmentVariable_SomethingThatShouldBeIgnored()
		{
			var value = _appSettings.ReplaceEnvironmentVariable("/test");
			Assert.AreEqual("/test", value);
		}

		[TestMethod]
		public void ReplaceEnvironmentVariable_Non_Existing_EnvVariable()
		{
			var value = _appSettings.ReplaceEnvironmentVariable("$sdhfdskfbndsfjb38");
			Assert.AreEqual("$sdhfdskfbndsfjb38", value);
		}
		
		[TestMethod]
		public void ReplaceEnvironmentVariable_Existing_EnvVariable()
		{
			Environment.SetEnvironmentVariable("test123456789","123456789");
			// should start with a dollar sign
			var value = _appSettings.ReplaceEnvironmentVariable("$test123456789");
			Assert.AreEqual("123456789", value);
		}

		[TestMethod]
		public void AppSettingsProviderTest_SqLiteFullPathStarskyCliTest()
		{
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.Sqlite;

			var datasource = _appSettings.SqLiteFullPath(
				"Data Source=data.db", Path.DirectorySeparatorChar + "starsky");
			Assert.AreEqual(true, datasource.Contains("data.db"));
			Assert.AreEqual(true, datasource.Contains("Data Source="));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AppSettingsProviderTest_SQLite_ExpectException()
		{
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.Sqlite;
			var datasource = _appSettings.SqLiteFullPath(string.Empty, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AppSettingsProviderTest_MySQL_ExpectException()
		{
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.Mysql;
			_appSettings.SqLiteFullPath(string.Empty, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AppSettingsProviderTest_StructureFails_withoutExtAndNoSlash()
		{
			_appSettings.Structure = "\\d";
			Assert.AreEqual("d", _appSettings.Structure);
		}

		[TestMethod]
		public void AppSettingsProviderTest_StructureFails_withExtAndNoSlash()
		{
			_appSettings.Structure = "\\d.ext";
			Assert.AreEqual("/\\d.ext", _appSettings.Structure);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AppSettingsProviderTest_StructureCheck_MissingFirstSlash()
		{
			AppSettings.StructureCheck("d/test.ext");
			// >= ArgumentException
		}

		[TestMethod]
		public void AppSettingsProviderTest_FolderWithFirstSlash()
		{
			AppSettings.StructureCheck("/d/dion.ext");
		}

		[TestMethod]
		public void AppSettingsProviderTest_NoFolderWithFirstSlash()
		{
			AppSettings.StructureCheck("/dion.ext");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AppSettingsProviderTest_NoFolderMissingFirstSlash()
		{
			AppSettings.StructureCheck("dion.ext");
			// >= ArgumentException
		}

		[TestMethod]
		public void AppSettingsNameNullTest()
		{
			var appSettings = new AppSettings {Name = null};
			Assert.AreEqual(string.Empty, appSettings.Name);
		}

		[TestMethod]
		public void AppSettingsNameNoWebSafeNameTest()
		{
			Assert.AreEqual("non-websafe-name/", 
				new AppSettings().GetWebSafeReplacedName("{name}","Non websafe name"));
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
				CameraTimeZoneInfo = TZConvert.GetTimeZoneInfo("Europe/Amsterdam")
			};

			// Linux: Europe/Amsterdam
			// Windows: W. Europe Standard Time

			bool isWindows = System.Runtime.InteropServices.RuntimeInformation
				.IsOSPlatform(OSPlatform.Windows);
			if ( isWindows )
			{
				Assert.IsTrue(appSettings.CameraTimeZone.Contains("W. Europe Standard Time"));
			}
			else
			{
				Assert.IsTrue(appSettings.CameraTimeZone.Contains("Europe/Amsterdam"));
			}

		}

		[TestMethod]
		public void AppSettingsGenerateSlugLengthCheck()
		{
			var slug = new AppSettings().GenerateSlug("1234567890123456789012345678901234567890123" +
				"456789012345678901234567890123456789012345678901234567890"+
				"456789012345678901234567890123456789012345678901234567890");
			// Length == 65
			Assert.AreEqual(65, slug.Length);
		}

		[TestMethod]
		public void GenerateSlug_Lowercase_Disabled()
		{
			var slug = new AppSettings().GenerateSlug("ABC", true, false);
			Assert.AreEqual("ABC", slug);
		}
		
		[TestMethod]
		public void GenerateSlug_Lowercase_Enabled()
		{
			var slug = new AppSettings().GenerateSlug("ABC");
			Assert.AreEqual("abc", slug);
		}
		
		[TestMethod]
		public void GenerateSlug_AllowAt_DisabledByDefault()
		{
			var slug = new AppSettings().GenerateSlug("test@123");
			Assert.AreEqual("test123", slug);
		}
		
		[TestMethod]
		public void GenerateSlug_AllowAt_Enabled()
		{
			var slug = new AppSettings().GenerateSlug("test@123", 
				false, true, true);
			Assert.AreEqual("test@123", slug);
		}
		
		[TestMethod]
		public void GenerateSlug_Trim()
		{
			var slug = new AppSettings().GenerateSlug("   abc   ");
			Assert.AreEqual("abc", slug);
		}
		
		[TestMethod]
		public void GenerateSlug_AllowUnderscore()
		{
			var slug = new AppSettings().GenerateSlug("a_b_c ", true);
			Assert.AreEqual("a_b_c", slug);
		}
		
		[TestMethod]
		public void GenerateSlug_Underscore_Disabled()
		{
			var slug = new AppSettings().GenerateSlug("a_b_c ");
			Assert.AreEqual("abc", slug);
		}

		[TestMethod]
		public void AppSettingsWebFtp_http()
		{
			var appSettings = new AppSettings {WebFtp = "https://google.com"};
			Assert.AreEqual(string.Empty,appSettings.WebFtp);
		}
		
		[TestMethod]
		public void AppSettingsWebFtp_FtpWithoutPassword()
		{
			var appSettings = new AppSettings();
			appSettings.WebFtp = "ftp://google.com";
			Assert.AreEqual(string.Empty,appSettings.WebFtp);
		}
		
		[TestMethod]
		public void AppSettingsWebFtp_FtpWithPassword()
		{
			var appSettings = new AppSettings();
			appSettings.WebFtp = "ftp://test:test@google.com";
			Assert.AreEqual("ftp://test:test@google.com",appSettings.WebFtp);
		}
		
		[TestMethod]
		public void SyncServiceRenameListItemsToDbStyleTest()
		{
			var appSettings = new AppSettings();

			var newImage = new CreateAnImage();
			_appSettings.StorageFolder = newImage.BasePath; // needs to have an / or \ at the end
			var inputList = new List<string>{ Path.DirectorySeparatorChar.ToString() };
			var expectedOutputList = new List<string>{ "/"};
			var output = appSettings.RenameListItemsToDbStyle(inputList);
			// list of files names that are starting with a filename (and not an / or \ )

			CollectionAssert.AreEqual(expectedOutputList,output);
		}

		[TestMethod]
		public void AppSettings_CloneToDisplay_hideSecurityItems()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql,
				WebFtp = "ftp://t:t@m.com",
				ApplicationInsightsInstrumentationKey = "token"
			};
			var display = appSettings.CloneToDisplay();
			Assert.AreEqual(display.DatabaseConnection,AppSettings.CloneToDisplaySecurityWarning);
			Assert.AreEqual(display.WebFtp,AppSettings.CloneToDisplaySecurityWarning);
			Assert.AreEqual(display.ApplicationInsightsInstrumentationKey,AppSettings.CloneToDisplaySecurityWarning);
		}
		
		[TestMethod]
		public void AppSettings_CloneToDisplay_hideSecurityItems_PublishProfiles()
		{
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"test", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
						},
						new AppSettingsPublishProfiles
						{
							Path = "value",
							Prepend = "value"
						}
					}}
				}
			};
			
			var display = appSettings.CloneToDisplay();
			// nr 0
			Assert.AreEqual(string.Empty,display.PublishProfiles["test"][0].Path);
			Assert.AreEqual(string.Empty,display.PublishProfiles["test"][0].Prepend);
			// nr 1
			Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,display.PublishProfiles["test"][1].Path);
			Assert.AreEqual(AppSettings.CloneToDisplaySecurityWarning,display.PublishProfiles["test"][1].Prepend);
		}

		[TestMethod]
		public void AppSettings_IsReadOnly_NullNoItemTest()
		{
			var appSettings = new AppSettings();
			Assert.AreEqual(false, appSettings.IsReadOnly(string.Empty));
		}

		[TestMethod]
		public void PublishProfiles_Null()
		{
			var appSettings = new AppSettings {PublishProfiles = null};
			Assert.AreEqual(0, appSettings.PublishProfiles.Count );
		}

		[TestMethod]
		public void AppVersionBuildDateTime()
		{
			var appVersionBuildDateTime = new AppSettings().AppVersionBuildDateTime;
			Assert.IsNotNull(appVersionBuildDateTime);
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
			var appSettings = new AppSettings {EnablePackageTelemetry = true};
			Assert.IsTrue(appSettings.EnablePackageTelemetry);
		}
		
		[TestMethod]
		public void EnablePackageTelemetry_False()
		{
			var appSettings = new AppSettings {EnablePackageTelemetry = true};
			Assert.IsTrue(appSettings.EnablePackageTelemetry);
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
		
	}
}
