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
	[Timeout(5000)]
	[ExpectedException(typeof(TimeoutException))]
	public async Task TimeoutAfter_CheckIfTimeouts_ExpectException()
	{
		await EndlessTest().TimeoutAfter(1);
		// expect TimeoutException
	}

	[TestMethod]
	[Timeout(5000)] // This ensures that the test itself has a timeout of 5 seconds
	public async Task TimeoutAfter_CheckIfTimeouts_WhenIsZero()
	{
		// Act & Assert
		var exception =
			await Assert.ThrowsExceptionAsync<TimeoutException>(() =>
				EndlessTest().TimeoutAfter(0));

		// Additional assertions (optional)
		Assert.AreEqual("timeout less than 0", exception.Message);
	}

	[TestMethod]
#if DEBUG
	[Timeout(4000)]
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
