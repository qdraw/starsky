using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class RunProcessTest
{
	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
	public async Task RunProcessAsync_ShellCommandSuccess_ReturnsTrue()
	{
		var logger = new FakeIWebLogger();
		var sut = new RunProcess(logger);

		var result = await sut.RunProcessAsync("/bin/sh", "-c \"echo ok\"");

		Assert.IsTrue(result);
		Assert.IsEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
	public async Task RunProcessAsync_ShellCommandNonZeroExit_ReturnsFalse_AndLogsError()
	{
		var logger = new FakeIWebLogger();
		var sut = new RunProcess(logger);

		var result = await sut.RunProcessAsync("/bin/sh", "-c \"echo err 1>&2; exit 9\"");

		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
		Assert.IsTrue(logger.TrackedExceptions[0].Item2?.Contains("exit code 9"));
	}

	[TestMethod]
	public async Task RunProcessAsync_InvalidExecutable_Throws()
	{
		var sut = new RunProcess(new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<Win32Exception>(
			() => sut.RunProcessAsync("__definitely_missing_executable__", ""));
	}
}
