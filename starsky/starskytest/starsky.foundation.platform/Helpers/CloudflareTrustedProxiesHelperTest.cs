using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class CloudflareTrustedProxiesHelperTest
{
	[TestMethod]
	public void GetTrustedProxies_NullAppSettings_ReturnsEmpty()
	{
		var result = CloudflareTrustedProxiesHelper.GetTrustedProxies(null);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void GetTrustedProxies_CloudflareDisabled_OnlyReturnsConfiguredEntries()
	{
		var appSettings = new AppSettings
		{
			TrustedProxies = new List<string> { "172.17.0.1" }, TrustedProxiesCloudflare = false
		};

		var result = CloudflareTrustedProxiesHelper.GetTrustedProxies(appSettings);

		Assert.HasCount(1, result);
		Assert.AreEqual("172.17.0.1", result[0]);
	}

	[TestMethod]
	public void GetTrustedProxies_CloudflareEnabled_IncludesCloudflareRanges()
	{
		var appSettings = new AppSettings
		{
			TrustedProxies = new List<string> { "172.17.0.1" }, TrustedProxiesCloudflare = true
		};

		var result = CloudflareTrustedProxiesHelper.GetTrustedProxies(appSettings);

		Assert.Contains("172.17.0.1", result);
		Assert.Contains("173.245.48.0/20", result);
		Assert.Contains("2400:cb00::/32", result);
		Assert.HasCount(1 + CloudflareTrustedProxiesHelper.IpRanges.Count, result);
	}

	[TestMethod]
	public void GetTrustedProxies_DuplicateEntries_AreDeduplicated()
	{
		var appSettings = new AppSettings
		{
			TrustedProxies = new List<string> { "173.245.48.0/20" }, TrustedProxiesCloudflare = true
		};

		var result = CloudflareTrustedProxiesHelper.GetTrustedProxies(appSettings);

		Assert.ContainsSingle(p => p == "173.245.48.0/20", result);
	}

	[TestMethod]
	public void IpRanges_AreNotEmpty()
	{
		Assert.IsNotEmpty(CloudflareTrustedProxiesHelper.IpRanges);
	}
}
