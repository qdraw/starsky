using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Helpers
{
	[TestClass]
	public class AppSettingsCompareHelperTest
	{
		[TestMethod]
		public void NewObject()
		{
			var input = new AppSettings();
			AppSettingsCompareHelper.Compare(input,null);

			Assert.AreEqual(input.Structure, new AppSettings().Structure);
		}
		
		[TestMethod]
		public void StringCompare()
		{
			var source = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Sqlite,
				DatabaseConnection = "Data Source=source"
			};
			
			var to = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Sqlite,
				DatabaseConnection = "Data Source=to"
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.DatabaseConnection, to.DatabaseConnection);
		}
		
		[TestMethod]
		public void BoolCompare()
		{
			var source = new AppSettings
			{
				Verbose = true
			};
			
			var to = new AppSettings
			{
				Verbose = false
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.Verbose, to.Verbose);
		}
		
		[TestMethod]
		public void ListStringCompare()
		{
			var source = new AppSettings
			{
				ReadOnlyFolders = new List<string>{"/test"}
			};
			
			var to = new AppSettings
			{
				ReadOnlyFolders = new List<string>{"/test2"}
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.ReadOnlyFolders.FirstOrDefault(), to.ReadOnlyFolders.FirstOrDefault());
		}
		
		[TestMethod]
		public void ListStringCompare_Same()
		{
			var source = new AppSettings
			{
				ReadOnlyFolders = new List<string>{"/same"}
			};
			
			var to = new AppSettings
			{
				ReadOnlyFolders = new List<string>{"/same"}
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.ReadOnlyFolders.FirstOrDefault(), to.ReadOnlyFolders.FirstOrDefault());
		}
		
		[TestMethod]
		public void DatabaseTypeListCompare()
		{
			var source = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Sqlite
			};
			
			var to = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.DatabaseType, to.DatabaseType);
		}
		
		[TestMethod]
		public void ListAppSettingsPublishProfilesCompare()
		{
			var source = new AppSettings
			{
				PublishProfiles = new List<AppSettingsPublishProfiles>
				{
					new AppSettingsPublishProfiles{Folder = "test"}
				}
			};
			
			var to = new AppSettings
			{
				PublishProfiles = new List<AppSettingsPublishProfiles>
				{
					new AppSettingsPublishProfiles{Folder = "test2"}
				}
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.PublishProfiles, to.PublishProfiles);
		}
		
		[TestMethod]
		public void ListAppSettingsPublishProfilesCompare_Same()
		{
			var source = new AppSettings
			{
				PublishProfiles = new List<AppSettingsPublishProfiles>
				{
					new AppSettingsPublishProfiles{Folder = "same"}
				}
			};
			
			var to = new AppSettings
			{
				PublishProfiles = new List<AppSettingsPublishProfiles>
				{
					new AppSettingsPublishProfiles{Folder = "same"}
				}
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.PublishProfiles, to.PublishProfiles);
		}
	}
}
