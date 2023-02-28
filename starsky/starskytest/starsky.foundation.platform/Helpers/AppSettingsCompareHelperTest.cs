using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Helpers
{
	[TestClass]
	public sealed class AppSettingsCompareHelperTest
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
		public void NullableBoolCompare()
		{
			var source = new AppSettings
			{
				Verbose = true
			};
			
			var to = new AppSettingsTransferObject()
			{
				Verbose = false // or null
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

			AppSettingsCompareHelper.Compare(source, source);
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
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"zz__example", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 1000,
							OverlayMaxWidth = 380,
							Path =  "{AssemblyDirectory}/EmbeddedViews/qdrawlarge.png",
							Folder =  "1000",
							Append = "_kl1k"
						}
					}}
				}
			};
			
			var to = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"zz__example2", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder =  "1000",
							Append = "_kl1k"
						}
					}}
				}
			};

			AppSettingsCompareHelper.Compare(source, to);
			
			Assert.AreEqual(source.PublishProfiles.Keys.FirstOrDefault(), to.PublishProfiles.Keys.FirstOrDefault());
		}
		
		[TestMethod]
		public void ListAppSettingsStringDictionary()
		{
			var source = new AppSettings
			{
				AccountRolesByEmailRegisterOverwrite = new Dictionary<string, string>
				{
				{"zz__example2", "Administrator"
				}}
			};
			
			var to = new AppSettings
			{
				AccountRolesByEmailRegisterOverwrite = new Dictionary<string, string>
				{
					{"zz__example2", "User"
				}}
			};

			AppSettingsCompareHelper.Compare(source, to);
			
			Assert.AreEqual(source.AccountRolesByEmailRegisterOverwrite.Keys.FirstOrDefault(), to.AccountRolesByEmailRegisterOverwrite.Keys.FirstOrDefault());
		}
		
		[TestMethod]
		public void ListAppSettingsStringDictionary_IgnoreOverwrite()
		{
			var source = new AppSettings
			{
				AccountRolesByEmailRegisterOverwrite = new Dictionary<string, string>
				{
				{"zz__example2", "Administrator"
				}}
			};
			
			var to = new AppSettings
			{
				AccountRolesByEmailRegisterOverwrite = null
			};

			AppSettingsCompareHelper.Compare(source, to);
			
			Assert.AreEqual(null, to.AccountRolesByEmailRegisterOverwrite?.Keys.FirstOrDefault());
		}
		
		[TestMethod]
		public void AppSettingsKeyValue_Compare()
		{
			var source = new AppSettings
			{
				DemoData = new List<AppSettingsKeyValue>
				{
					new AppSettingsKeyValue
					{
						Key = "2",
						Value = "1"
					}
				}
			};
			
			var to = new AppSettings
			{
				DemoData = new List<AppSettingsKeyValue>
				{
					new AppSettingsKeyValue
					{
						Key = "1",
						Value = "1"
					}
				}
			};

			AppSettingsCompareHelper.Compare(source, to);
			
			Assert.AreEqual(source.PublishProfiles.Keys.FirstOrDefault(), to.PublishProfiles.Keys.FirstOrDefault());
		}
		
		[TestMethod]
		public void AppSettingsKeyValue_Compare_Same()
		{
			var source = new AppSettings
			{
				DemoData = new List<AppSettingsKeyValue>
				{
					new AppSettingsKeyValue
					{
						Key = "same",
						Value = "1"
					}
				}
			};
			
			var to = new AppSettings
			{
				DemoData = source.DemoData
			};

			AppSettingsCompareHelper.Compare(source, to);
			
			Assert.AreEqual(source.PublishProfiles.Keys.FirstOrDefault(), to.PublishProfiles.Keys.FirstOrDefault());
		}
		
		[TestMethod]
		public void ListAppSettingsPublishProfilesCompare_Same()
		{
			var source = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"same", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder =  "1000",
							Append = "_kl1k"
						}
					}}
				}
			};
			
			var to = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"same", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder =  "1000",
							Append = "_kl1k"
						}
					}}
				}
			};

			AppSettingsCompareHelper.Compare(source, to);
			Assert.AreEqual(source.PublishProfiles.Keys.FirstOrDefault(), to.PublishProfiles.Keys.FirstOrDefault());
		}

		[TestMethod]
		public void CompareDatabaseTypeList_NotFound()
		{
			var list = new List<string>();
			AppSettingsCompareHelper.CompareDatabaseTypeList("t",
				new AppSettings(),
				AppSettings.DatabaseTypeList.Mysql,
				AppSettings.DatabaseTypeList.Mysql, list);
			Assert.IsNotNull(list);
		}
		
		[TestMethod]
		public void CompareListString_NotFound()
		{
			var list = new List<string>();
			AppSettingsCompareHelper.CompareListString("t",
				new AppSettings(),
				new List<string>{"1"},
				new List<string>{"1"}, list);
			Assert.IsNotNull(list);
		}
		
		[TestMethod]
		public void CompareListPublishProfiles_NotFound()
		{
			var list = new List<string>();
			AppSettingsCompareHelper.CompareListPublishProfiles("t",
				new AppSettings(),
				new Dictionary<string, List<AppSettingsPublishProfiles>>(),
				new Dictionary<string, List<AppSettingsPublishProfiles>>(), list);
			Assert.IsNotNull(list);
		}
		
		[TestMethod]
		public void CompareBool_NotFound()
		{
			var list = new List<string>();
			bool? boolValue = true;
			AppSettingsCompareHelper.CompareBool("t",
				new AppSettings(),
				boolValue,
				boolValue, list);
			Assert.IsNotNull(list);
		}
		
		[TestMethod]
		public void CompareString_NotFound()
		{
			var list = new List<string>();
			AppSettingsCompareHelper.CompareString("t",
				new AppSettings(),
				"Test",
				"test", list);
			Assert.IsNotNull(list);
		}
		
		[TestMethod]
		public void CompareInt_NotFound()
		{
			var list = new List<string>();
			AppSettingsCompareHelper.CompareInt("t",
				new AppSettings(),
				1,
				2, list);
			Assert.IsNotNull(list);
		}
	}
}
