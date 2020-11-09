using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;

namespace starskytest.starsky.foundation.platform.Extensions
{
	[TestClass]
	public class TimeoutTaskAfterTest
	{
		private async Task<bool> EndlessTest(int duration = 10000)
		{
			 await Task.Delay(duration);
			 return true;
		}
		
		[TestMethod]
		[Timeout(500)]
		[ExpectedException(typeof(TimeoutException))]
		public async Task CheckIfTimeouts()
		{
			await EndlessTest().TimeoutAfter(3);
		}
		
		[TestMethod]
		public async Task CheckIfSuccess()
		{
			await EndlessTest(1).TimeoutAfter(10);
		}
		
	}
}
