using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller.Helpers;

[TestClass]
public class WatchServiceNameTest
{
	[TestMethod]
	public void WatchServiceName_Overridden_IsRunningTest_True_AppendsTestSuffix()
	{
		var sut = new TestableWatchServiceName(true);
		// Reverse DNS name explicitly appends "-test" when IsRunningTest is true
		Assert.EndsWith("-test", sut.GetReverseDnsName(), sut.GetReverseDnsName());
		// SystemD name/current implementation appends "-test" when IsRunningTest is true
		Assert.Contains("-test", sut.GetSystemDName(), sut.GetSystemDName());
	}

	[TestMethod]
	public void WatchServiceName_Overridden_IsRunningTest_False_NoTestSuffix()
	{
		var sut = new TestableWatchServiceName(false);
		Assert.DoesNotEndWith("-test", sut.GetReverseDnsName(), sut.GetReverseDnsName());
		Assert.DoesNotContain("-test", sut.GetSystemDName(), sut.GetSystemDName());
	}

	// Small testable subclass to override IsRunningTest for unit tests
	private sealed class TestableWatchServiceName(bool isRunningTest) : WatchServiceName
	{
		protected override bool IsRunningTest()
		{
			return isRunningTest;
		}
	}
}
