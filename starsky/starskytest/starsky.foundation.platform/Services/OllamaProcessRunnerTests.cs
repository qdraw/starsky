using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Services;

[TestClass]
public sealed class OllamaProcessRunnerTests
{
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task RunProcessWithOutputAsync_DotNetVersion_ShouldSucceed()
	{
		var sut = new OllamaProcessRunner(new FakeIWebLogger());
		var result = await sut.RunProcessWithOutputAsync("dotnet", "--version",
			cancellationToken: TestContext.CancellationToken);

		Assert.IsTrue(result.Success);
		Assert.IsGreaterThan(0, result.Output.Length);
	}

	[TestMethod]
	public async Task RunProcessWithOutputAsync_InvalidExecutable_ShouldFail()
	{
		var sut = new OllamaProcessRunner(new FakeIWebLogger());
		var result = await sut.RunProcessWithOutputAsync("__missing_executable__", "",
			cancellationToken: TestContext.CancellationToken);

		Assert.IsFalse(result.Success);
		Assert.IsGreaterThan(0, result.Error.Length);
	}

	[TestMethod]
	public async Task StartServeAsync_InvalidExecutable_ShouldFail()
	{
		var sut = new OllamaProcessRunner(new FakeIWebLogger());
		var started = await sut.StartServeAsync("__missing_executable__",
			cancellationToken: TestContext.CancellationToken);

		Assert.IsFalse(started);
		Assert.IsFalse(sut.IsServeRunning);
	}
}




