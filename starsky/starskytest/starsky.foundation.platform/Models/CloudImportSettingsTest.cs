using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class CloudImportSettingsTest
{
	[TestMethod]
	[DataRow(true, 0, 10, true, DisplayName = "Enabled, Minutes > 0")] // Should be included
	[DataRow(true, 5, 0, true, DisplayName = "Enabled, Hours > 0")] // Should be included
	[DataRow(true, 0, 0, false, DisplayName = "Enabled, Both 0")] // Should NOT be included
	[DataRow(false, 0, 10, false, DisplayName = "Disabled, Minutes > 0")] // Should NOT be included
	[DataRow(false, 5, 0, false, DisplayName = "Disabled, Hours > 0")] // Should NOT be included
	[DataRow(false, 0, 0, false, DisplayName = "Disabled, Both 0")] // Should NOT be included
	public void GetEnabledSyncFrequencyProviders_Theory(bool enabled, int hours, double minutes,
		bool shouldBeIncluded)
	{
		var settings = new CloudImportSettings
		{
			Providers =
			[
				new CloudImportProviderSettings
				{
					Enabled = enabled,
					SyncFrequencyHours = hours,
					SyncFrequencyMinutes = minutes
				}
			]
		};

		var result = settings.GetEnabledSyncFrequencyProviders();
		if ( shouldBeIncluded )
		{
			Assert.HasCount(1, result,
				$"Expected provider to be included for enabled={enabled}, hours={hours}, minutes={minutes}");
		}
		else
		{
			Assert.IsEmpty(result,
				$"Expected provider to be excluded for enabled={enabled}, hours={hours}, minutes={minutes}");
		}
	}

	[TestMethod]
	public void GetEnabledSyncFrequencyProviders_MultipleProviders_MixedResults()
	{
		var settings = new CloudImportSettings
		{
			Providers =
			[
				new CloudImportProviderSettings
				{
					Enabled = true, SyncFrequencyHours = 1, SyncFrequencyMinutes = 0
				}, // included
				new CloudImportProviderSettings
				{
					Enabled = true, SyncFrequencyHours = 0, SyncFrequencyMinutes = 5
				}, // included
				new CloudImportProviderSettings
				{
					Enabled = true, SyncFrequencyHours = 0, SyncFrequencyMinutes = 0
				}, // not included
				new CloudImportProviderSettings
				{
					Enabled = false, SyncFrequencyHours = 1, SyncFrequencyMinutes = 0
				} // not included
			]
		};
		var result = settings.GetEnabledSyncFrequencyProviders();
		Assert.HasCount(2, result, "Should only include enabled providers with non-zero frequency");
	}

	[TestMethod]
	[DataRow(true, 0, 10, true, DisplayName = "Enabled, Minutes > 0")]
	[DataRow(true, 5, 0, true, DisplayName = "Enabled, Hours > 0")]
	[DataRow(true, 0, 0, true, DisplayName = "Enabled, Both 0")]
	[DataRow(false, 0, 10, false, DisplayName = "Disabled, Minutes > 0")]
	[DataRow(false, 5, 0, false, DisplayName = "Disabled, Hours > 0")]
	[DataRow(false, 0, 0, false, DisplayName = "Disabled, Both 0")]
	public void GetEnabledProviders_Theory(bool enabled, int hours, double minutes,
		bool shouldBeIncluded)
	{
		var settings = new CloudImportSettings
		{
			Providers =
			[
				new CloudImportProviderSettings
				{
					Enabled = enabled,
					SyncFrequencyHours = hours,
					SyncFrequencyMinutes = minutes
				}
			]
		};

		var result = settings.GetEnabledProviders();
		if ( shouldBeIncluded )
		{
			Assert.HasCount(1, result,
				$"Expected provider to be included for enabled={enabled}, hours={hours}, minutes={minutes}");
		}
		else
		{
			Assert.IsEmpty(result,
				$"Expected provider to be excluded for enabled={enabled}, hours={hours}, minutes={minutes}");
		}
	}
}
