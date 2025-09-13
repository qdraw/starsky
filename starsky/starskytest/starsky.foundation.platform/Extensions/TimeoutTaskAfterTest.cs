using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;

namespace starskytest.starsky.foundation.platform.Extensions;

[TestClass]
public sealed class TimeoutTaskAfterTest
{
	private static async Task<bool> EndlessTest(int duration = 10000)
	{
		await Task.Delay(duration);
		return true;
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task TimeoutAfter_CheckIfTimeouts_ExpectException()
	{
		// Act & Assert
		var exception =
			await Assert.ThrowsExactlyAsync<TimeoutException>(() =>
				EndlessTest().TimeoutAfter(1));

		// Additional assertions (optional)
		Assert.AreEqual("[TimeoutTaskAfter] task operation has timed out", exception.Message);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)] // This ensures that the test itself has a timeout of 5 seconds
	public async Task TimeoutAfter_CheckIfTimeouts_WhenIsZero()
	{
		// Act & Assert
		var exception =
			await Assert.ThrowsExactlyAsync<TimeoutException>(() =>
				EndlessTest().TimeoutAfter(0));

		// Additional assertions (optional)
		Assert.AreEqual("timeout less than 0", exception.Message);
	}

	[TestMethod]
#if DEBUG
	[Timeout(4000, CooperativeCancellation = true)]
#else
		[Timeout(20000)]
#endif
	public async Task TimeoutAfter_CheckIfSuccess()
	{
#if DEBUG
		Console.WriteLine("DEBUG");
#else
			Console.WriteLine("RELEASE");
#endif
		Assert.IsTrue(await EndlessTest(1).TimeoutAfter(4000));
	}
}
