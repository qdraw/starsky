using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
[DoNotParallelize]
public class SentrySettingsTest
{
	[TestMethod]
	[DataRow("Development", null, "Development")]
	[DataRow(null, "Staging", "Staging")]
	[DataRow(" ", "Staging", "Staging")]
	[DataRow(null, null, "production")]
	public void GetEnvironmentName_TheoryTests(string? environmentName,
		string? environmentVariable, string expected)
	{
		// Arrange
		Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentVariable);
		var settings = new SentrySettings { Environment = environmentName };

		// Act
		var result = settings.GetEnvironmentName();

		// Assert
		Assert.AreEqual(expected, result);

		// Cleanup
		Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
	}
}
