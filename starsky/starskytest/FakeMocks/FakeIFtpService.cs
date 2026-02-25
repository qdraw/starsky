using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.webftppublish.Interfaces;
using starsky.feature.webftppublish.Models;

namespace starskytest.FakeMocks;

public class FakeIFtpService : IFtpService
{
	public bool RunResult { get; set; } = true;
	public FtpPublishManifestModel? ManifestResult { get; set; } = new();
	public string LastPath { get; private set; } = string.Empty;
	public string LastSlug { get; private set; } = string.Empty;

	public Task<FtpPublishManifestModel?> IsValidZipOrFolder(string inputFullFileDirectoryOrZip)
	{
		LastPath = inputFullFileDirectoryOrZip;
		return Task.FromResult(ManifestResult);
	}

	public bool Run(string parentDirectoryOrZipFile, string slug,
		Dictionary<string, bool> copyContent)
	{
		LastPath = parentDirectoryOrZipFile;
		LastSlug = slug;
		return RunResult;
	}
}