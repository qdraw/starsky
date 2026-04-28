using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
		var detectedLock = new object();
		var detectedTcs =
			new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		sut.MountDetected += (_, args) =>
		{
			lock ( detectedLock )
			{
				detected.Add(args.MountPath);
			}

			detectedTcs.TrySetResult(true);
		};
		sut.SetRunning(true);

		var pollingTask = Task.Run(sut.RunPollingFallbackForTest, TestContext.CancellationToken);
		bool hasDetectedMount;
		try
		{
			await detectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5),
				TestContext.CancellationToken);
			hasDetectedMount = true;
		}
		catch ( TimeoutException )
		{
			hasDetectedMount = false;
		}
		finally
		{
			sut.SetRunning(false);
			await pollingTask;
		}

		Assert.IsTrue(hasDetectedMount,
			$"Timed out waiting for mount detection. GetMountedVolumesCallCount={sut.GetMountedVolumesCallCount}");
		lock ( detectedLock )
		{
			CollectionAssert.AreEqual(new List<string> { "/mnt/camera" }, detected);
		}
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
		var reachedSecondRead = false;
		try
		{
			var sw = Stopwatch.StartNew();
			while ( sw.Elapsed < TimeSpan.FromSeconds(3) )
			{
				if ( sut.GetMountedVolumesCallCount >= 2 )
				{
					reachedSecondRead = true;
					break;
				}

				await Task.Delay(10, TestContext.CancellationToken);
			}
		}
		finally
		{
			sut.SetRunning(false);
			await pollingTask;
		}

		Assert.IsTrue(reachedSecondRead,
			$"Polling did not reach a second read in time. GetMountedVolumesCallCount={sut.GetMountedVolumesCallCount}");
		Assert.IsGreaterThanOrEqualTo(2, sut.GetMountedVolumesCallCount);
	}

	private sealed class TestBaseMountWatcher : BaseMountWatcher
	{
		private int _getMountedVolumesCallCount;

		public TestBaseMountWatcher() : base(new FakeIWebLogger(), 10)
		{
		}

		public Queue<List<string>> Snapshots { get; set; } = new();
		public int ThrowOnReadNumber { get; set; }
		public int GetMountedVolumesCallCount => Volatile.Read(ref _getMountedVolumesCallCount);

		public override void Start()
		{
		}

		public override void Stop()
		{
		}

		public override List<string> GetMountedVolumes()
		{
			var callCount = Interlocked.Increment(ref _getMountedVolumesCallCount);

			if ( ThrowOnReadNumber > 0 && callCount == ThrowOnReadNumber )
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
