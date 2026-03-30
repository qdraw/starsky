using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.import.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class CameraMountWatcherServiceTest
{
	[TestMethod]
	public async Task StartStopStatus_HappyFlow()
	{
		var eventSource = new FakeMountEventSource();
		var detector = new FakeCameraStorageDetector(["/Volumes/CARD"]);
		var runner = new FakeImporterCliRunner();
		var writer = new FakeMountWatcherLogWriter();
		var importService = new FakeMountImportService();
		var appSettings = new AppSettings
		{
			MountWatcherEnabled = true,
			MountWatcherProcessModel = AppSettings.MountWatcherImportProcessModel.Cli
		};
		var sut = new CameraMountWatcherService(eventSource, detector, runner, importService,
			writer, new FakeIWebLogger(), appSettings);

		var started = await sut.StartAsync();
		Assert.IsTrue(started.Running);

		eventSource.Trigger(new MountAppearedEventModel { MountRootPath = "/Volumes/CARD" });
		await Task.Delay(20, TestContext.CancellationToken);

		var status = sut.GetStatus();
		Assert.AreEqual(1, status.TotalImportsTriggered);

		var stopped = await sut.StopAsync();
		Assert.IsFalse(stopped.Running);
		Assert.AreEqual(1, eventSource.StartCount);
		Assert.AreEqual(1, eventSource.StopCount);
	}

	[TestMethod]
	public async Task StartAsync_DisabledInSettings_DoesNotStart()
	{
		var eventSource = new FakeMountEventSource();
		var sut = new CameraMountWatcherService(eventSource,
			new FakeCameraStorageDetector([]), new FakeImporterCliRunner(),
			new FakeMountImportService(), new FakeMountWatcherLogWriter(),
			new FakeIWebLogger(), new AppSettings());

		var status = await sut.StartAsync();

		Assert.IsFalse(status.Running);
		StringAssert.Contains(status.LastResult ?? string.Empty, "disabled");
	}

	[TestMethod]
	public async Task StartAsync_WhenEventSourceUnavailable_ReturnsNotAvailable()
	{
		var eventSource = new FakeMountEventSource { StartReturns = false };
		var sut = new CameraMountWatcherService(eventSource,
			new FakeCameraStorageDetector([]), new FakeImporterCliRunner(),
			new FakeMountImportService(), new FakeMountWatcherLogWriter(),
			new FakeIWebLogger(), new AppSettings { MountWatcherEnabled = true });

		var status = await sut.StartAsync();

		Assert.IsFalse(status.Running);
		StringAssert.Contains(status.LastResult ?? string.Empty, "not available");
	}

	[TestMethod]
	public async Task MountAppeared_FilteredOutByMountPath_DoesNotImport()
	{
		var eventSource = new FakeMountEventSource();
		var runner = new FakeImporterCliRunner();
		var sut = new CameraMountWatcherService(eventSource,
			new FakeCameraStorageDetector(["/Volumes/CARD"]), runner,
			new FakeMountImportService(), new FakeMountWatcherLogWriter(),
			new FakeIWebLogger(), new AppSettings
			{
				MountWatcherEnabled = true,
				MountWatcherProcessModel = AppSettings.MountWatcherImportProcessModel.Cli
			});

		await sut.StartAsync();
		eventSource.Trigger(new MountAppearedEventModel { MountRootPath = "/Volumes/OTHER" });
		await Task.Delay(20, TestContext.CancellationToken);

		Assert.AreEqual(0, runner.RunCount);
	}

	[TestMethod]
	public async Task StartStop_AreIdempotent()
	{
		var eventSource = new FakeMountEventSource();
		var sut = new CameraMountWatcherService(eventSource,
			new FakeCameraStorageDetector([]), new FakeImporterCliRunner(),
			new FakeMountImportService(), new FakeMountWatcherLogWriter(),
			new FakeIWebLogger(), new AppSettings { MountWatcherEnabled = true });

		await sut.StartAsync();
		await sut.StartAsync();
		await sut.StopAsync();
		await sut.StopAsync();

		Assert.AreEqual(1, eventSource.StartCount);
		Assert.AreEqual(1, eventSource.StopCount);
	}

	[TestMethod]
	public async Task AutoMode_FallsBackToCli_WhenInProcessFails()
	{
		var eventSource = new FakeMountEventSource();
		var runner = new FakeImporterCliRunner();
		var importService = new FakeMountImportService { ThrowInImporter = true };
		var sut = new CameraMountWatcherService(eventSource,
			new FakeCameraStorageDetector(["/Volumes/CARD"]), runner,
			importService, new FakeMountWatcherLogWriter(),
			new FakeIWebLogger(), new AppSettings
			{
				MountWatcherEnabled = true,
				MountWatcherProcessModel = AppSettings.MountWatcherImportProcessModel.Auto
			});

		await sut.StartAsync();
		eventSource.Trigger(new MountAppearedEventModel { MountRootPath = "/Volumes/CARD" });
		await Task.Delay(20, TestContext.CancellationToken);

		Assert.AreEqual(1, runner.RunCount);
		Assert.AreEqual(1, importService.Calls);
	}

	private sealed class FakeMountEventSource : IMountEventSource
	{
		public bool IsRunning { get; private set; }
		public int StartCount { get; private set; }
		public int StopCount { get; private set; }
		public bool StartReturns { get; set; } = true;
		public event Action<MountAppearedEventModel>? MountAppeared;

		public bool Start()
		{
			StartCount++;
			if ( !StartReturns )
			{
				return false;
			}

			IsRunning = true;
			return true;
		}

		public void Stop()
		{
			if ( !IsRunning )
			{
				return;
			}

			StopCount++;
			IsRunning = false;
		}

		public void Trigger(MountAppearedEventModel model)
		{
			MountAppeared?.Invoke(model);
		}

		public void Dispose()
		{
			Stop();
		}
	}

	private sealed class FakeCameraStorageDetector(IEnumerable<string> items) : ICameraStorageDetector
	{
		public IEnumerable<string> FindCameraStorages()
		{
			return items;
		}
	}

	private sealed class FakeImporterCliRunner : IImporterCliRunner
	{
		public int RunCount { get; private set; }

		public Task<ImporterCliRunResult> RunCameraImportAsync(CancellationToken cancellationToken = default)
		{
			RunCount++;
			return Task.FromResult(new ImporterCliRunResult { Success = true, Message = "ok" });
		}
	}

	private sealed class FakeMountImportService : IImport
	{
		public int Calls { get; private set; }
		public bool ThrowInImporter { get; set; }

		public Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList,
			ImportSettingsModel importSettings)
		{
			return Task.FromResult(new List<ImportIndexItem>());
		}

		public Task<List<ImportIndexItem>> Importer(IEnumerable<string> inputFullPathList,
			ImportSettingsModel importSettings)
		{
			Calls++;
			if ( ThrowInImporter )
			{
				throw new InvalidOperationException("in-process failed");
			}

			return Task.FromResult(inputFullPathList.Select(path => new ImportIndexItem
			{
				FilePath = path,
				Status = ImportStatus.Ok
			}).ToList());
		}
	}

	private sealed class FakeMountWatcherLogWriter : IMountWatcherLogWriter
	{
		public Task WriteAsync(string eventName, object payload, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}

	public TestContext TestContext { get; set; }
}








