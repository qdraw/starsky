using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.Base;

[TestClass]
public class LoggingTestBase
{
	public TestContext TestContext { get; set; }

	// This runs **before each test**
	[TestInitialize]
	public async Task TestInitializeAsync()
	{
		Console.WriteLine(
			$"[{DateTime.Now:HH:mm:ss.fff}] Test {TestContext.TestName} initializing...");
		await Task.Yield(); // just to allow async context
		Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Test {TestContext.TestName} initialized");
	}

	// This runs **after each test**
	[TestCleanup]
	public async Task TestCleanupAsync()
	{
		Console.WriteLine(
			$"[{DateTime.Now:HH:mm:ss.fff}] Test {TestContext.TestName} cleaning up...");

		try
		{
			// Wrap cleanup in a timeout to catch hangs
			var cleanupTask = CleanupAsync();
			if ( await Task.WhenAny(cleanupTask, Task.Delay(10000)) != cleanupTask )
			{
				Console.WriteLine($"⚠️ Test {TestContext.TestName} cleanup timed out!");
			}
			else
			{
				Console.WriteLine(
					$"[{DateTime.Now:HH:mm:ss.fff}] Test {TestContext.TestName} cleanup finished");
			}
		}
		catch ( Exception ex )
		{
			Console.WriteLine($"⚠️ Test {TestContext.TestName} cleanup threw exception: {ex}");
		}
	}

	protected virtual async Task CleanupAsync()
	{
		// Override in derived tests if you need async cleanup
		await Task.Yield();
	}
}
