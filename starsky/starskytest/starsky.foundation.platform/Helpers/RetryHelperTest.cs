using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class RetryHelperTest
{
	[TestMethod]
	public void RetrySucceed()
	{
		var count = 0;

		bool Test()
		{
			count++;
			if ( count == 2 )
			{
				return true;
			}

			throw new ApplicationException();
		}

		var result = RetryHelper.Do(Test, TimeSpan.Zero);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void RetryFail_expect_AggregateException()
	{
		var count = 0;

		// Act & Assert
		var ex = Assert.ThrowsExactly<AggregateException>(() =>
		{
			RetryHelper.Do(Test, TimeSpan.Zero);
		});

		// Verify that the AggregateException contains the expected inner exceptions
		var innerExceptions = ex.InnerExceptions;
		Assert.Contains(e => e is FormatException, innerExceptions);
		Assert.Contains(e => e is ApplicationException, innerExceptions);
		return;

		bool Test()
		{
			if ( count == 2 )
			{
				throw new FormatException(); // <= combines with AggregateException
			}

			count++;
			throw new ApplicationException();
		}
	}

	[TestMethod]
	public void RetryFail_expect_ArgumentOutOfRangeException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RetryHelper.Do(Test,
			TimeSpan.Zero, 0));
		return;

		// Arrange
		static bool Test()
		{
			return true;
		}
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
			return count == 2 ? true : throw new ApplicationException();
		}

		var result = await RetryHelper.DoAsync(Test, TimeSpan.Zero);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task Async_RetryFail_expect_AggregateException()
	{
		var count = 0;

		// Act & Assert
		var ex = await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
			await AssertTest());

		// Verify the AggregateException contains the expected inner exceptions
		Assert.Contains(e => e is FormatException, ex.InnerExceptions);
		Assert.Contains(e => e is ApplicationException, ex.InnerExceptions);
		return;

		async Task AssertTest()
		{
			var result = await RetryHelper.DoAsync(Test, TimeSpan.Zero);
			Assert.IsTrue(result); // This will not be reached if exception is thrown
		}

		Task<bool> Test()
		{
			if ( count == 2 )
			{
				throw new FormatException(); // Combined with AggregateException
			}

			count++;
			throw new ApplicationException();
		}
	}

	[TestMethod]
	public async Task Async_RetryFail_expect_ArgumentOutOfRangeException()
	{
		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () =>
		{
			// should not be negative
			await RetryHelper.DoAsync(Test, TimeSpan.Zero, 0);
		});

		// Arrange
		static async Task<bool> Test()
		{
			// Just return true for the purpose of this test
			return await Task.FromResult(true);
		}
	}
}
