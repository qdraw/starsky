using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers;

[TestClass]
public sealed class PublishCliTest
{
	[TestMethod]
	public async Task Publisher_Help()
	{
		var console = new FakeConsoleWrapper();

		await new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(),
			new FakeIWebHtmlPublishService(),
			new AppSettings(), console, new FakeIWebLogger()).Publisher([
			"-h"
		]);

		Assert.IsTrue(console.WrittenLines.FirstOrDefault()
			?.Contains("Starsky WebHtml Cli ~ Help:"));
		Assert.IsTrue(console.WrittenLines.LastOrDefault()
			?.Contains("  use -v -help to show settings: "));
	}

	[TestMethod]
	public async Task Publisher_Default()
	{
		var console = new FakeConsoleWrapper();
		await new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(),
			new FakeIWebHtmlPublishService(),
			new AppSettings(), console, new FakeIWebLogger()).Publisher([
			""
		]);

		Assert.IsTrue(console.WrittenLines.FirstOrDefault()
			?.Contains("Please use the -p to add a path first"));
	}

	[TestMethod]
	public async Task Publisher_PathArg()
	{
		var console = new FakeConsoleWrapper();
		await new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(),
			new FakeIWebHtmlPublishService(),
			new AppSettings(), console, new FakeIWebLogger()).Publisher([
			"-p"
		]);

		Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("is not found"));
	}

	[TestMethod]
	public async Task Publisher_NoSettingsFileInFolder()
	{
		var console = new FakeConsoleWrapper();
		var fakeSelectorStorage =
			new FakeSelectorStorage(new FakeIStorage(new List<string> { "/test" }));

		await new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(),
			new FakeIWebHtmlPublishService(),
			new AppSettings(), console, new FakeIWebLogger()).Publisher([
			"-p", "/test"
		]);

		Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("done"));
	}

	[TestMethod]
	public async Task Publisher_WarnWhenAlreadyRun()
	{
		var console = new FakeConsoleWrapper();
		var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(
			new List<string> { Path.DirectorySeparatorChar + "test" },
			new List<string>
			{
				$"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}_settings.json"
			}));

		await new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(),
			new FakeIWebHtmlPublishService(),
			new AppSettings(), console, new FakeIWebLogger()).Publisher([
			"-p", Path.DirectorySeparatorChar + "test"
		]);

		// says something that settings json already has written
		Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("_settings.json"));
	}

	[TestMethod]
	public async Task Publisher_RunningDifferentProfile()
	{
		var console = new FakeConsoleWrapper();
		var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(
			new List<string> { Path.DirectorySeparatorChar + "test" },
			new List<string>()));

		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test1",
					new List<AppSettingsPublishProfiles>
					{
						new() { Path = Path.DirectorySeparatorChar + "test" }
					}
				},
				{
					"test2",
					new List<AppSettingsPublishProfiles>
					{
						new() { Path = Path.DirectorySeparatorChar + "test" }
					}
				}
			}
		};
		await new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(),
			new FakeIWebHtmlPublishService(),
			appSettings, console, new FakeIWebLogger()).Publisher([
			"-p", Path.DirectorySeparatorChar + "test",
			"--profile", "test2"
		]);

		Assert.IsTrue(console.WrittenLines.Exists(p => p.Contains("Running profile: test2")));
		Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("done"));
	}

	[TestMethod]
	public async Task Publisher_RunningNotFoundProfile()
	{
		var console = new FakeConsoleWrapper();
		var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(
			new List<string> { Path.DirectorySeparatorChar + "test" },
			new List<string>()));

		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test1",
					new List<AppSettingsPublishProfiles>
					{
						new() { Path = Path.DirectorySeparatorChar + "test" }
					}
				},
				{
					"test2",
					new List<AppSettingsPublishProfiles>
					{
						new() { Path = Path.DirectorySeparatorChar + "test" }
					}
				}
			}
		};
		await new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(),
			new FakeIWebHtmlPublishService(),
			appSettings, console, new FakeIWebLogger()).Publisher([
			"-p", Path.DirectorySeparatorChar + "test",
			"--profile", "not-found-profile"
		]);

		// defaults to first one
		Assert.IsTrue(console.WrittenLines.Exists(p =>
			p.Contains("Profile not found, uses default test1 use --profile to select one")));
		Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("done"));
	}
}
