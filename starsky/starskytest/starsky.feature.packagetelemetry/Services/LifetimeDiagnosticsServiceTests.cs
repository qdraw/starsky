using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.packagetelemetry.Services;
using starsky.foundation.database.Diagnostics;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.packagetelemetry.Services;

[TestClass]
public class LifetimeDiagnosticsServiceTests
{
	[TestMethod]
	public async Task AddOrUpdateApplicationStopping_ValidStartTime_ReturnsDiagnosticsItem()
	{
		// Arrange
		var fakeDiagnosticsService = new FakeDiagnosticsService();
		var fakeLogger = new FakeIWebLogger();
		var service = new LifetimeDiagnosticsService(fakeDiagnosticsService, fakeLogger);
		var startTime = DateTime.UtcNow.AddMinutes(-30);

		// Act
		var result = await service.AddOrUpdateApplicationStopping(startTime);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(nameof(DiagnosticsType.ApplicationStoppingLifetimeInMinutes), result.Key);
		Assert.AreEqual("30", result.Value);
	}

	[TestMethod]
	public async Task GetLastApplicationStoppingTimeInMinutes_ValidValue_ReturnsMinutes()
	{
		// Arrange
		var fakeDiagnosticsService = new FakeDiagnosticsService();
		fakeDiagnosticsService.SetItem(new DiagnosticsItem
		{
			Key = nameof(DiagnosticsType.ApplicationStoppingLifetimeInMinutes), Value = "45"
		});
		var fakeLogger = new FakeIWebLogger();
		var service = new LifetimeDiagnosticsService(fakeDiagnosticsService, fakeLogger);

		// Act
		var result = await service.GetLastApplicationStoppingTimeInMinutes();

		// Assert
		Assert.AreEqual(45, result);
	}

	[TestMethod]
	public async Task GetLastApplicationStoppingTimeInMinutes_InvalidValue_ReturnsMinusOne()
	{
		// Arrange
		var fakeDiagnosticsService = new FakeDiagnosticsService();
		fakeDiagnosticsService.SetItem(new DiagnosticsItem
		{
			Key = nameof(DiagnosticsType.ApplicationStoppingLifetimeInMinutes),
			Value = "invalid"
		});
		var fakeLogger = new FakeIWebLogger();
		var service = new LifetimeDiagnosticsService(fakeDiagnosticsService, fakeLogger);

		// Act
		var result = await service.GetLastApplicationStoppingTimeInMinutes();

		// Assert
		Assert.AreEqual(-1, result);
	}
}
