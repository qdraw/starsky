using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.MountWatcher;

[TestClass]
public sealed class BaseMountWatcherTest
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public void BaseMountWatcher_OnMountDetected_RaisesEventWithPath()
	{
		var sut = new TestBaseMountWatcher();
		string? detectedPath = null;
		sut.MountDetected += (_, args) => detectedPath = args.MountPath;

		sut.RaiseMount("/mnt/camera");

		Assert.AreEqual("/mnt/camera", detectedPath);
	}

	[TestMethod]
	[Timeout(8000, CooperativeCancellation = true)]
	public async Task BaseMountWatcher_RunPollingFallback_DetectsAndRemovesMounts()
	{
		var sut = new TestBaseMountWatcher
		{
			Snapshots = new Queue<List<string>>([
				new List<string>(),
				new List<string> { "/mnt/camera" },
				new List<string>()
			])
		};
		var detected = new List<string>();
		sut.MountDetected += (_, args) => detected.Add(args.MountPath);
		sut.SetRunning(true);

		var pollingTask = Task.Run(sut.RunPollingFallbackForTest, TestContext.CancellationToken);
		await Task.Delay(4300, TestContext.CancellationToken);
		sut.SetRunning(false);
		await pollingTask;

		CollectionAssert.AreEqual(new List<string> { "/mnt/camera" }, detected);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task BaseMountWatcher_RunPollingFallback_HandlesReadExceptions()
	{
		var sut = new TestBaseMountWatcher
		{
			Snapshots = new Queue<List<string>>([
				new List<string>()
			]),
			ThrowOnReadNumber = 2
		};
		sut.SetRunning(true);

		var pollingTask = Task.Run(sut.RunPollingFallbackForTest, TestContext.CancellationToken);
		await Task.Delay(20, TestContext.CancellationToken);
		sut.SetRunning(false);
		await pollingTask;

		Assert.AreEqual(2, sut.GetMountedVolumesCallCount);
	}

	private sealed class TestBaseMountWatcher : BaseMountWatcher
	{
		public TestBaseMountWatcher() : base(new FakeIWebLogger(),10)
		{
		}

		public Queue<List<string>> Snapshots { get; set; } = new();
		public int ThrowOnReadNumber { get; set; }
		public int GetMountedVolumesCallCount { get; private set; }

		public override void Start()
		{
		}

		public override void Stop()
		{
		}

		public override List<string> GetMountedVolumes()
		{
			GetMountedVolumesCallCount++;
			if ( ThrowOnReadNumber > 0 && GetMountedVolumesCallCount == ThrowOnReadNumber )
			{
				throw new InvalidOperationException("simulated");
			}

			if ( Snapshots.Count > 0 )
			{
				return Snapshots.Dequeue();
			}

			return [];
		}

		public void SetRunning(bool value)
		{
			IsRunning = value;
		}

		public void RunPollingFallbackForTest()
		{
			RunPollingFallback();
		}

		public void RaiseMount(string mountPath)
		{
			OnMountDetected(mountPath);
		}
	}
}
