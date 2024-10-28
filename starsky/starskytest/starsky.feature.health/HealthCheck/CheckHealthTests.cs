using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck.Service;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.HealthCheck;

[TestClass]
public class CheckHealthTests
{
	private static Exception CreateExceptionWithStackTrace(string message, string? stackTrace)
	{
		var exception = new Exception(message);
		if ( stackTrace == null )
		{
			return exception;
		}

		var stackTraceField = typeof(Exception).GetField("_stackTraceString",
			BindingFlags.NonPublic | BindingFlags.Instance);
		stackTraceField?.SetValue(exception, stackTrace);
		return exception;
	}

	private static HealthReport CreateHealthReport(string key, string description,
		string? exceptionMessage,
		string? stackTrace)
	{
		var healthReportEntry = new HealthReportEntry(
			HealthStatus.Unhealthy,
			description,
			TimeSpan.Zero,
			exceptionMessage != null
				? CreateExceptionWithStackTrace(exceptionMessage, stackTrace)
				: null,
			null);

		var healthReport = new HealthReport(
			new Dictionary<string, HealthReportEntry> { { key, healthReportEntry } },
			TimeSpan.Zero);
		return healthReport;
	}

	[DataTestMethod]
	[DataRow("HealthyCheck", null, null, null)]
	[DataRow("HealthyCheck", "Healthy", null, null)]
	[DataRow("UnhealthyCheck", "Unhealthy", "Some error occurred", "Stack trace")]
	[DataRow("TimeoutCheck", "timeout", "Timeout exception", "Timeout stack trace")]
	[DataRow("VersionMismatchCheck", "Version mismatch", "Version mismatch error",
		"Version mismatch stack trace")]
	public void CreateHealthEntryLog_LogError_ShouldLogCorrectMessage(string key,
		string description,
		string? exceptionMessage, string? stackTrace)
	{
		// Arrange
		var logger = new FakeIWebLogger();

		var healthReport = CreateHealthReport(key, description, exceptionMessage, stackTrace);
		var checkHealthService = new CheckHealthService(null!, logger);

		// Act
		checkHealthService.CreateHealthEntryLog(healthReport);

		// Assert
		var loggedError = logger.TrackedExceptions.FirstOrDefault();
		Assert.IsNotNull(loggedError);
		Assert.IsTrue(loggedError.Item2?.Contains($"HealthCheck {key} failed {description}"));
		if ( exceptionMessage != null )
		{
			Assert.IsTrue(loggedError.Item2?.Contains(exceptionMessage));
		}

		if ( stackTrace != null )
		{
			Assert.IsTrue(loggedError.Item2?.Contains(stackTrace));
		}
	}

	[DataTestMethod]
	[DataRow("HealthyCheck", null, null, null)]
	[DataRow("HealthyCheck", "Healthy", null, null)]
	[DataRow("UnhealthyCheck", "Unhealthy", "Some error occurred", "Stack trace")]
	[DataRow("TimeoutCheck", "timeout", "Timeout exception", "Timeout stack trace")]
	[DataRow("VersionMismatchCheck", "Version mismatch", "Version mismatch error",
		"Version mismatch stack trace")]
	public void CreateHealthEntryLog_Result(string key, string description,
		string? exceptionMessage, string? stackTrace)
	{
		// Arrange
		var logger = new FakeIWebLogger();

		var healthReport = CreateHealthReport(key, description, exceptionMessage, stackTrace);
		var checkHealthService = new CheckHealthService(null!, logger);

		// Act
		var result = checkHealthService.CreateHealthEntryLog(healthReport);

		// Assert
		Assert.AreEqual(healthReport.Status == HealthStatus.Healthy, result.IsHealthy);
		Assert.AreEqual(healthReport.TotalDuration, result.TotalDuration);
		Assert.AreEqual(healthReport.Entries.Count, result.Entries.Count);
		Assert.AreEqual(healthReport.Entries.First().Key, result.Entries.FirstOrDefault()?.Name);
		Assert.AreEqual(healthReport.Entries.First().Value.Duration,
			result.Entries.FirstOrDefault()?.Duration);
		Assert.AreEqual(healthReport.Entries.First().Value.Status == HealthStatus.Healthy,
			result.Entries.FirstOrDefault()?.IsHealthy);
		Assert.AreEqual(healthReport.Entries.First().Value.Description ?? string.Empty,
			result.Entries.FirstOrDefault()?.Description);
	}

	[TestMethod]
	public async Task CheckHealthAsyncWithTimeout_ShouldTimeout()
	{
		var result =
			await new CheckHealthService(new FakeHealthCheckService(true), new FakeIWebLogger())
				.CheckHealthWithTimeoutAsync(-1);
		Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
	}

	[TestMethod]
	public async Task CheckHealthAsyncWithTimeout_ShouldSucceed()
	{
		var result =
			await new CheckHealthService(new FakeHealthCheckService(true), new FakeIWebLogger())
				.CheckHealthWithTimeoutAsync();
		Assert.AreEqual(HealthStatus.Healthy, result.Status);
	}

	[TestMethod]
	public async Task CheckHealthAsyncWithTimeout_IgnoreCheckIfCachedInputIsHealthy()
	{
		var entry = new HealthReportEntry(
			HealthStatus.Healthy,
			"timeout",
			TimeSpan.FromMilliseconds(1),
			null,
			null);

		var cachedItem = new Dictionary<string, object>
		{
			{
				"health", new HealthReport(
					new Dictionary<string, HealthReportEntry> { { "timeout", entry } },
					TimeSpan.FromMilliseconds(0))
			}
		};

		var result = await new CheckHealthService(new FakeHealthCheckService(false),
			new FakeIWebLogger(), new FakeMemoryCache(cachedItem)).CheckHealthWithTimeoutAsync();

		Assert.AreEqual(HealthStatus.Healthy, result.Status);
	}

	[DataTestMethod]
	[DataRow(true, HealthStatus.Healthy, true)]
	[DataRow(false, HealthStatus.Unhealthy, false)]
	public async Task CheckHealthAsyncWithTimeout_ShouldSetCache(bool isHealthy,
		HealthStatus healthStatus, bool expectCache)
	{
		var cache = new MemoryCache(new MemoryCacheOptions());
		var result = await new CheckHealthService(new FakeHealthCheckService(isHealthy),
			new FakeIWebLogger(), cache).CheckHealthWithTimeoutAsync();

		cache.TryGetValue(CheckHealthService.CacheKey, out var cachedResult);

		if ( expectCache )
		{
			Assert.AreEqual(result, cachedResult);
		}
		else
		{
			Assert.IsNull(cachedResult);
		}

		Assert.AreEqual(healthStatus, result.Status);
	}
}
