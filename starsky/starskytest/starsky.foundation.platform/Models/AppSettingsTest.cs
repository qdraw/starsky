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
	public class AppSettingsProviderTest
	{
		private readonly AppSettings _appSettings;

		public AppSettingsProviderTest()
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
				Assert.AreEqual(appSettings.CameraTimeZone.Contains("W. Europe Standard Time"),
					true);
			}
			else
			{
				Assert.AreEqual(appSettings.CameraTimeZone.Contains("Europe/Amsterdam"), true);
			}

		}

		[TestMethod]
		public void AppSettingsGenerateSlugLenghtCheck()
		{
			var appSettings = new AppSettings();
			string slug = appSettings.GenerateSlug("12345678901234567890123456789012345678901234567890");
			// lenght == 45
			Assert.AreEqual(slug.Length,45);
		}


		[TestMethod]
		public void AppSettingsWebFtp_http()
		{
			var appSettings = new AppSettings();
			appSettings.WebFtp = "https://google.com";
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
	}
}
