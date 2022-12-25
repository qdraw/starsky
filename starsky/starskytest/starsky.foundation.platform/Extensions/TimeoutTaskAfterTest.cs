using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;

namespace starskytest.starsky.foundation.platform.Extensions
{
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
		[Timeout(5000)]
		[ExpectedException(typeof(TimeoutException))]
		public async Task TimeoutAfter_CheckIfTimeouts_WhenIsZero()
		{
			// zero is not allowed as time
			await EndlessTest().TimeoutAfter(0);
			// expect TimeoutException
		}
		
		// [TestMethod]
		// disabled
		[Timeout(5000)]
		public async Task TimeoutAfter_CheckIfSuccess()
		{
			Assert.IsTrue(await EndlessTest(1).TimeoutAfter(100));
		}
	}
}
