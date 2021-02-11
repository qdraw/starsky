using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers
{
	[TestClass]
	public class RetryHelperTest
	{
		[TestMethod]
		public void RetrySucceed()
		{
			var count = 0;
			bool Test()
			{
				count++;
				if ( count == 2)
				{
					return true;
				}
				throw new ApplicationException();
			}
			
			var result = RetryHelper.Do(Test, TimeSpan.Zero);
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		[ExpectedException(typeof(AggregateException))]
		public void RetryFail_expect_AggregateException()
		{
			var count = 0;
			bool Test()
			{
				if ( count == 2)
				{
					throw new FormatException(); // <= does combine it with AggregateException
				}
				count++;
				throw new ApplicationException();
			}
			
			var result = RetryHelper.Do(Test, TimeSpan.Zero);
			Assert.IsTrue(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void RetryFail_expect_ArgumentOutOfRangeException()
		{
			bool Test()
			{
				return true;
			}
			// should not be negative
			RetryHelper.Do(Test, TimeSpan.Zero, 0);
		}

		[TestMethod]
		public async Task Async_RetrySucceed()
		{
			var count = 0;
#pragma warning disable 1998
			async Task<bool> Test()
#pragma warning restore 1998
			{
				count++;
				if ( count == 2)
				{
					return true;
				}
				throw new ApplicationException();
			}
			
			var result = await RetryHelper.DoAsync(Test, TimeSpan.Zero);
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		[ExpectedException(typeof(AggregateException))]
		public async Task Async_RetryFail_expect_AggregateException()
		{
			var count = 0;
#pragma warning disable 1998
			async Task<bool> Test()
#pragma warning restore 1998
			{
				if ( count == 2)
				{
					throw new FormatException(); // <= does combine it with AggregateException
				}
				count++;
				throw new ApplicationException();
			}
			
			var result = await RetryHelper.DoAsync(Test, TimeSpan.Zero);
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public async Task Async_RetryFail_expect_ArgumentOutOfRangeException()
		{
#pragma warning disable 1998
			async Task<bool> Test()
#pragma warning restore 1998
			{
				return true;
			}
			// should not be negative
			await RetryHelper.DoAsync(Test, TimeSpan.Zero, 0);
		}
	}
}
