using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller;

[TestClass]
public sealed class WindowsServiceInstallerTest
{
	[TestMethod]
	public async Task InstallAsync_Dll_UsesDotnetHostInCreateCommand()
	{
		var calls = new List<(string fileName, string args)>();
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			},
			_ => Task.CompletedTask);

		var result = await sut.InstallAsync("C:/apps/starskymountwatchercli.dll");

		Assert.IsTrue(result);
		Assert.HasCount(1, calls);
		Assert.AreEqual("sc.exe", calls[0].fileName);
		Assert.Contains("create \"", calls[0].args);
		Assert.Contains("dotnet.exe", calls[0].args);
		Assert.Contains("starskymountwatchercli.dll", calls[0].args);
	}

	[TestMethod]
	public async Task InstallAsync_Exe_UsesExecutablePathInCreateCommand()
	{
		var calls = new List<(string fileName, string args)>();
		var sut = new WindowsServiceInstaller(new FakeIWebLogger(),
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			},
			_ => Task.CompletedTask);

		var result = await sut.InstallAsync("C:/apps/starskymountwatchercli.exe");

		Assert.IsTrue(result);
		Assert.HasCount(1, calls);
		Assert.Contains("starskymountwatchercli.exe", calls[0].args);
		Assert.IsFalse(calls[0].args.Contains("dotnet.exe", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public async Task StartAsync_RetriesOnce_WhenFirstStartFails()
	{
		var calls = new List<(string fileName, string args)>();
		var delays = new List<int>();
		var invocation = 0;
		var sut = new WindowsServiceInstaller(new FakeIWebLogger(),
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				invocation++;
				return Task.FromResult(invocation > 1);
			},
			ms =>
			{
				delays.Add(ms);
				return Task.CompletedTask;
			});

		var result = await sut.StartAsync();

		Assert.IsTrue(result);
		Assert.HasCount(2, calls);
		Assert.HasCount(1, delays);
		Assert.AreEqual(2000, delays[0]);
		Assert.Contains("start", calls[0].args);
		Assert.Contains("start", calls[1].args);
	}

	[TestMethod]
	public async Task UninstallAsync_StopsThenDeletesService()
	{
		var calls = new List<(string fileName, string args)>();
		var sut = new WindowsServiceInstaller(new FakeIWebLogger(),
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			},
			_ => Task.CompletedTask);

		var result = await sut.UninstallAsync();

		Assert.IsTrue(result);
		Assert.HasCount(2, calls);
		Assert.Contains("stop", calls[0].args);
		Assert.Contains("delete", calls[1].args);
	}

	[TestMethod]
	public async Task StopAsync_ReturnsFalse_WhenRunnerThrows()
	{
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => throw new InvalidOperationException("boom"),
			_ => Task.CompletedTask);

		var result = await sut.StopAsync();

		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}
}
