using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class AppSettingsCompareHelperTest
{
	[TestMethod]
	public void NewObject()
	{
		var input = new AppSettings();
		AppSettingsCompareHelper.Compare(input);

		Assert.AreEqual(input.Name, new AppSettings().Name);
	}

	[TestMethod]
	public void Structure()
	{
		var source = new AppSettings
		{
			Structure =
				new AppSettingsStructureModel("/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext")
				{
					Rules =
					[
						new StructureRule
						{
							Pattern = "/yyyy_MM_dd/clip/yyyyMMdd_HHmmss_{filenamebase}.ext",
							Conditions = new StructureRuleConditions
							{
								ImageFormats = [ExtensionRolesHelper.ImageFormat.mp4]
							}
						}
					]
				}
		};

		var to = new AppSettings
		{
			Structure =
				new AppSettingsStructureModel("/mm/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext")
				{
					Rules =
					[
						new StructureRule
						{
							Pattern = "/yyyy_MM_dd/test/yyyyMMdd_HHmmss_{filenamebase}.ext",
							Conditions = new StructureRuleConditions
							{
								ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg]
							}
						}
					]
				}
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.Structure.DefaultPattern, to.Structure.DefaultPattern);
		Assert.AreEqual(source.Structure.Rules[0].Conditions, to.Structure.Rules[0].Conditions);
		Assert.AreEqual(source.Structure.Rules[0].Pattern, to.Structure.Rules[0].Pattern);
	}

	[TestMethod]
	public void Structure_Ignore_DefaultOption()
	{
		var source = new AppSettings
		{
			Structure =
				new AppSettingsStructureModel("/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext")
				{
					Rules =
					[
						new StructureRule
						{
							Pattern = "/yyyy_MM_dd/clip/yyyyMMdd_HHmmss_{filenamebase}.ext",
							Conditions = new StructureRuleConditions
							{
								ImageFormats = [ExtensionRolesHelper.ImageFormat.mp4]
							}
						}
					]
				}
		};

		var to = new AppSettings { Structure = new AppSettingsStructureModel() };

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual("/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext",
			source.Structure.DefaultPattern);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4,
			source.Structure.Rules[0].Conditions.ImageFormats[0]);
		Assert.AreEqual("/yyyy_MM_dd/clip/yyyyMMdd_HHmmss_{filenamebase}.ext",
			source.Structure.Rules[0].Pattern);
	}

	[TestMethod]
	public void AppSettingsImportTransformationModel()
	{
		var source = new AppSettings
		{
			ImportTransformation =
				new AppSettingsImportTransformationModel
				{
					Rules =
					[
						new TransformationRule
						{
							ColorClass = ColorClassParser.Color.Superior,
							Conditions = new TransformationConditions
							{
								ImageFormats = [ExtensionRolesHelper.ImageFormat.png],
								Origin = "test-1"
							}
						}
					]
				}
		};

		var to = new AppSettings
		{
			ImportTransformation =
				new AppSettingsImportTransformationModel
				{
					Rules =
					[
						new TransformationRule
						{
							ColorClass = ColorClassParser.Color.Extras,
							Conditions = new TransformationConditions
							{
								ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
								Origin = "test"
							}
						}
					]
				}
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.ImportTransformation.Rules[0].ColorClass,
			to.ImportTransformation.Rules[0].ColorClass);
		Assert.AreEqual(source.ImportTransformation.Rules[0].Conditions.Origin,
			to.ImportTransformation.Rules[0].Conditions.Origin);
		Assert.AreEqual(source.ImportTransformation.Rules[0].Conditions.ImageFormats[0],
			to.ImportTransformation.Rules[0].Conditions.ImageFormats[0]);
	}

	[TestMethod]
	public void AppSettingsImportTransformationModel_Ignore_DefaultOption()
	{
		var source = new AppSettings
		{
			ImportTransformation = new AppSettingsImportTransformationModel
			{
				Rules =
				[
					new TransformationRule
					{
						ColorClass = ColorClassParser.Color.Superior,
						Conditions = new TransformationConditions
						{
							ImageFormats = [ExtensionRolesHelper.ImageFormat.png],
							Origin = "test-1"
						}
					}
				]
			}
		};

		var to = new AppSettings
		{
			ImportTransformation = new AppSettingsImportTransformationModel()
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(ColorClassParser.Color.Superior,
			source.ImportTransformation.Rules[0].ColorClass);
		Assert.AreEqual("test-1",
			source.ImportTransformation.Rules[0].Conditions.Origin);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.png,
			source.ImportTransformation.Rules[0].Conditions.ImageFormats[0]);
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
		var source = new AppSettings { Verbose = true };

		var to = new AppSettingsTransferObject
		{
			Verbose = false // or null
		};

		AppSettingsCompareHelper.Compare(source, to);
		Assert.AreEqual(source.Verbose, to.Verbose);
	}

	[TestMethod]
	public void ListStringCompare()
	{
		var source = new AppSettings { ReadOnlyFolders = new List<string> { "/test" } };

		var to = new AppSettings { ReadOnlyFolders = new List<string> { "/test2" } };

		AppSettingsCompareHelper.Compare(source, to);
		Assert.AreEqual(source.ReadOnlyFolders.FirstOrDefault(),
			to.ReadOnlyFolders.FirstOrDefault());
	}

	[TestMethod]
	public void ListStringCompare_Same()
	{
		var source = new AppSettings { ReadOnlyFolders = new List<string> { "/same" } };

		var to = new AppSettings { ReadOnlyFolders = new List<string> { "/same" } };

		var compare = AppSettingsCompareHelper.Compare(source, source);

		Assert.AreEqual(source.ReadOnlyFolders.FirstOrDefault(),
			to.ReadOnlyFolders.FirstOrDefault());
		Assert.IsEmpty(compare);
	}

	[TestMethod]
	public void DatabaseTypeListCompare()
	{
		var source = new AppSettings { DatabaseType = AppSettings.DatabaseTypeList.Sqlite };

		var to = new AppSettings { DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase };

		AppSettingsCompareHelper.Compare(source, to);
		Assert.AreEqual(source.DatabaseType, to.DatabaseType);
	}

	[TestMethod]
	public void DesktopCollectionsOpenCompare()
	{
		var source = new AppSettings
		{
			DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Raw
		};

		var to = new AppSettings { DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Jpeg };

		AppSettingsCompareHelper.Compare(source, to);
		Assert.AreEqual(source.DesktopCollectionsOpen, to.DesktopCollectionsOpen);
	}

	[TestMethod]
	public void DesktopCollectionsOpenCompare_DefaultIgnore()
	{
		var source = new AppSettings
		{
			DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Raw
		};

		var to = new AppSettings
		{
			DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Default
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(CollectionsOpenType.RawJpegMode.Raw, source.DesktopCollectionsOpen);
	}

	[TestMethod]
	public void ListAppSettingsPublishProfilesCompare()
	{
		var source = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"zz__example", new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 1000,
							OverlayMaxWidth = 380,
							Path =
								"{AssemblyDirectory}/EmbeddedViews/qdrawlarge.png",
							Folder = "1000",
							Append = "_kl1k"
						}
					}
				}
			}
		};

		var to = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"zz__example2",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder = "1000",
							Append = "_kl1k"
						}
					}
				}
			}
		};

		var compare = AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.PublishProfiles.Keys.FirstOrDefault(),
			to.PublishProfiles.Keys.FirstOrDefault());
		Assert.AreEqual("PublishProfiles".ToLowerInvariant(), compare.FirstOrDefault());
	}

	[TestMethod]
	public void ListAppSettingsStringDictionary_Changed()
	{
		var source = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite = new Dictionary<string, string>
			{
				{ "zz__example2", "Administrator" }
			}
		};

		var to = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite = new Dictionary<string, string>
			{
				{ "zz__example2", "User" }
			}
		};

		var compare = AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.AccountRolesByEmailRegisterOverwrite.Keys.FirstOrDefault(),
			to.AccountRolesByEmailRegisterOverwrite.Keys.FirstOrDefault());
		Assert.AreEqual("AccountRolesByEmailRegisterOverwrite".ToLowerInvariant(),
			compare.FirstOrDefault());
	}

	[TestMethod]
	public void ListAppSettingsStringDictionary_Equal()
	{
		var source = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite = new Dictionary<string, string>
			{
				{ "zz__example2", "Administrator" }
			}
		};

		var to = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite =
				source.AccountRolesByEmailRegisterOverwrite
		};

		var compare = AppSettingsCompareHelper.Compare(source, to);
		var expected = source.AccountRolesByEmailRegisterOverwrite.Keys.FirstOrDefault();
		var actual = to.AccountRolesByEmailRegisterOverwrite.Keys.FirstOrDefault();
		Assert.AreEqual(expected, actual);
		Assert.IsEmpty(compare);
	}

	[TestMethod]
	public void ListAppSettingsStringDictionary_IgnoreOverwrite()
	{
		var source = new AppSettings
		{
			AccountRolesByEmailRegisterOverwrite = new Dictionary<string, string>
			{
				{ "zz__example2", "Administrator" }
			}
		};

		var to = new AppSettings { AccountRolesByEmailRegisterOverwrite = null };

		AppSettingsCompareHelper.Compare(source, to);

		var actual = to.AccountRolesByEmailRegisterOverwrite?.Keys.FirstOrDefault();
		Assert.IsNull(actual);
	}

	[TestMethod]
	public void KeyValuePairStringString_Changed()
	{
		var source = new AppSettings
		{
			DemoData = new List<AppSettingsKeyValue> { new() { Key = "1", Value = "2" } }
		};

		var to = new AppSettings
		{
			DemoData = new List<AppSettingsKeyValue> { new() { Key = "3", Value = "4" } }
		};

		var compare = AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.DemoData.FirstOrDefault()?.Key,
			to.DemoData.FirstOrDefault()?.Key);
		Assert.AreEqual(source.DemoData.FirstOrDefault()?.Value,
			to.DemoData.FirstOrDefault()?.Value);

		Assert.AreEqual("DemoData".ToLowerInvariant(), compare.FirstOrDefault());
	}

	[TestMethod]
	public void KeyValuePairStringString_Equal()
	{
		var source = new AppSettings
		{
			DemoData = new List<AppSettingsKeyValue> { new() { Key = "1", Value = "2" } }
		};

		var to = new AppSettings { DemoData = source.DemoData };

		var compare = AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.DemoData.FirstOrDefault()?.Key,
			to.DemoData.FirstOrDefault()?.Key);
		Assert.AreEqual(source.DemoData.FirstOrDefault()?.Value,
			to.DemoData.FirstOrDefault()?.Value);
		Assert.IsEmpty(compare);
	}

	[TestMethod]
	public void KeyValuePairStringString_IgnoreOverwrite()
	{
		var source = new AppSettings
		{
			DemoData = new List<AppSettingsKeyValue> { new() { Key = "1", Value = "2" } }
		};

		var to = new AppSettings { DemoData = null! };

		var compare = AppSettingsCompareHelper.Compare(source, to);

		if ( to.DemoData != null )
		{
			Assert.Fail("to.DemoData should be null");
		}

		Assert.IsEmpty(compare);
	}

	[TestMethod]
	public void AppSettingsKeyValue_Compare()
	{
		var source = new AppSettings
		{
			DemoData = new List<AppSettingsKeyValue> { new() { Key = "2", Value = "1" } }
		};

		var to = new AppSettings
		{
			DemoData = new List<AppSettingsKeyValue> { new() { Key = "1", Value = "1" } }
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.DemoData.FirstOrDefault(),
			to.DemoData?.FirstOrDefault());
	}

	[TestMethod]
	public void AppSettingsKeyValue_Compare_Same()
	{
		var source = new AppSettings
		{
			DemoData = new List<AppSettingsKeyValue> { new() { Key = "same", Value = "1" } }
		};

		var to = new AppSettings { DemoData = source.DemoData };

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.DemoData.FirstOrDefault(),
			to.DemoData.FirstOrDefault());
	}

	[TestMethod]
	public void ListAppSettingsPublishProfilesCompare_Same()
	{
		var source = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"same",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder = "1000",
							Append = "_kl1k"
						}
					}
				}
			}
		};

		var to = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"same",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder = "1000",
							Append = "_kl1k"
						}
					}
				}
			}
		};

		AppSettingsCompareHelper.Compare(source, to);
		Assert.AreEqual(source.PublishProfiles.Keys.FirstOrDefault(),
			to.PublishProfiles.Keys.FirstOrDefault());
	}

	[TestMethod]
	public void AppSettingsPublishProfilesDefaults_Difference()
	{
		var source = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults()
		};

		var to = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				ProfileFeatures = new ProfileFeatures
				{
					Optimization = new Optimization { Enabled = true },
					Publishing = new Publishing { Enabled = true }
				},
				Optimizers =
				[
					new Optimizer
					{
						Id = "Optimizer1",
						Enabled = true,
						ImageFormats =
						[
							ExtensionRolesHelper.ImageFormat.jpg,
							ExtensionRolesHelper.ImageFormat.png
						],
						Options = new OptimizerOptions { Quality = 80 }
					}
				],
				PublishTargets = new PublishTargets { Ftp = new FtpTarget { Enabled = true } }
			}
		};

		AppSettingsCompareHelper.Compare(source, to);
		Assert.AreEqual(source.PublishProfilesDefaults,
			to.PublishProfilesDefaults);
	}

	[TestMethod]
	public void AppSettingsPublishProfilesDefaults_Same()
	{
		var source = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				ProfileFeatures = new ProfileFeatures
				{
					Optimization = new Optimization { Enabled = true },
					Publishing = new Publishing { Enabled = true }
				}
			}
		};

		var to = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				ProfileFeatures = new ProfileFeatures
				{
					Optimization = new Optimization { Enabled = true },
					Publishing = new Publishing { Enabled = true }
				}
			}
		};

		AppSettingsCompareHelper.Compare(source, to);
		Assert.AreEqual(source.PublishProfilesDefaults.ProfileFeatures.Optimization.Enabled,
			to.PublishProfilesDefaults.ProfileFeatures.Optimization.Enabled);
		Assert.AreEqual(source.PublishProfilesDefaults.ProfileFeatures.Publishing.Enabled,
			to.PublishProfilesDefaults.ProfileFeatures.Publishing.Enabled);
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
			new List<string> { "1" },
			new List<string> { "1" }, list);
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

	[TestMethod]
	public void OpenTelemetrySettings()
	{
		var source = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				Header = "source/test",
				TracesEndpoint = "source/traces",
				TracesHeader = "source/traces",
				MetricsEndpoint = "source/metrics",
				MetricsHeader = "source/metrics",
				LogsEndpoint = "source/logs",
				LogsHeader = "source/logs"
			}
		};

		var to = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				Header = "to/test",
				TracesEndpoint = "to/traces",
				TracesHeader = "to/traces",
				MetricsEndpoint = "to/metrics",
				MetricsHeader = "to/metrics",
				LogsEndpoint = "to/logs",
				LogsHeader = "to/logs"
			}
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual(source.OpenTelemetry.Header, to.OpenTelemetry.Header);
		Assert.AreEqual(source.OpenTelemetry.TracesEndpoint, to.OpenTelemetry.TracesEndpoint);
		Assert.AreEqual(source.OpenTelemetry.TracesHeader, to.OpenTelemetry.TracesHeader);
		Assert.AreEqual(source.OpenTelemetry.MetricsEndpoint, to.OpenTelemetry.MetricsEndpoint);
		Assert.AreEqual(source.OpenTelemetry.MetricsHeader, to.OpenTelemetry.MetricsHeader);
		Assert.AreEqual(source.OpenTelemetry.LogsEndpoint, to.OpenTelemetry.LogsEndpoint);
		Assert.AreEqual(source.OpenTelemetry.LogsHeader, to.OpenTelemetry.LogsHeader);
	}

	[TestMethod]
	public void OpenTelemetrySettings_Ignore_DefaultOption()
	{
		var source = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				Header = "source/test",
				TracesEndpoint = "source/traces",
				TracesHeader = "source/traces",
				MetricsEndpoint = "source/metrics",
				MetricsHeader = "source/metrics",
				LogsEndpoint = "source/logs",
				LogsHeader = "source/logs"
			}
		};

		var to = new AppSettings { OpenTelemetry = new OpenTelemetrySettings() };

		AppSettingsCompareHelper.Compare(source, to);

		Assert.AreEqual("source/test", source.OpenTelemetry.Header);
		Assert.AreEqual("source/traces", source.OpenTelemetry.TracesEndpoint);
		Assert.AreEqual("source/metrics", source.OpenTelemetry.MetricsEndpoint);
		Assert.AreEqual("source/logs", source.OpenTelemetry.LogsEndpoint);
	}

	[TestMethod]
	public void AppSettingsDefaultEditorApplication()
	{
		var source = new AppSettings
		{
			DefaultDesktopEditor =
			[
				new AppSettingsDefaultEditorApplication
				{
					ImageFormats =
					[
						ExtensionRolesHelper.ImageFormat.bmp,
						ExtensionRolesHelper.ImageFormat.jpg
					],
					ApplicationPath = "source/test"
				}
			]
		};

		var to = new AppSettings
		{
			DefaultDesktopEditor =
			[
				new AppSettingsDefaultEditorApplication
				{
					ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
					ApplicationPath = "to/test"
				}
			]
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.HasCount(source.DefaultDesktopEditor.Count, to.DefaultDesktopEditor);
		Assert.AreEqual(source.DefaultDesktopEditor[0].ApplicationPath,
			to.DefaultDesktopEditor[0].ApplicationPath);
		Assert.AreEqual(source.DefaultDesktopEditor[0].ImageFormats,
			to.DefaultDesktopEditor[0].ImageFormats);
	}

	[TestMethod]
	public void AppSettingsDefaultEditorApplication_Ignore_DefaultOption()
	{
		var source = new AppSettings
		{
			DefaultDesktopEditor =
			[
				new AppSettingsDefaultEditorApplication
				{
					ImageFormats =
					[
						ExtensionRolesHelper.ImageFormat.bmp,
						ExtensionRolesHelper.ImageFormat.jpg
					],
					ApplicationPath = "source/test"
				}
			]
		};

		var to = new AppSettings { DefaultDesktopEditor = [] };

		AppSettingsCompareHelper.Compare(source, to);

		Assert.IsEmpty(to.DefaultDesktopEditor);
	}

	[TestMethod]
	public void AppSettingsCloudImport()
	{
		var source = new AppSettings { CloudImport = new CloudImportSettings { Providers = [] } };

		var to = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "to/test",
						Provider = "Dropbox",
						RemoteFolder = "/to/test",
						Enabled = true,
						SyncFrequencyHours = 1,
						DeleteAfterImport = false
					}
				]
			}
		};

		AppSettingsCompareHelper.Compare(source, to);

		Assert.HasCount(source.CloudImport.Providers.Count, to.CloudImport.Providers);
		Assert.AreEqual(source.CloudImport.Providers[0].Id,
			to.CloudImport.Providers[0].Id);
		Assert.AreEqual(source.CloudImport.Providers[0].Provider,
			to.CloudImport.Providers[0].Provider);
		Assert.AreEqual(source.CloudImport.Providers[0].RemoteFolder,
			to.CloudImport.Providers[0].RemoteFolder);
		Assert.AreEqual(source.CloudImport.Providers[0].Enabled,
			to.CloudImport.Providers[0].Enabled);
		Assert.AreEqual(source.CloudImport.Providers[0].SyncFrequencyHours,
			to.CloudImport.Providers[0].SyncFrequencyHours);
		Assert.AreEqual(source.CloudImport.Providers[0].DeleteAfterImport,
			to.CloudImport.Providers[0].DeleteAfterImport);
	}

	[TestMethod]
	public void AppSettingsCloudImport_Ignore_DefaultOption()
	{
		var source = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "to/test",
						Provider = "Dropbox",
						RemoteFolder = "/to/test",
						Enabled = true,
						SyncFrequencyHours = 1,
						DeleteAfterImport = false
					}
				]
			}
		};

		var to = new AppSettings { CloudImport = new CloudImportSettings { Providers = [] } };

		AppSettingsCompareHelper.Compare(source, to);

		Assert.IsEmpty(to.CloudImport.Providers);
	}
}
