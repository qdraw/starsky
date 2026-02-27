using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.webftppublish.Interfaces;
using starsky.feature.webftppublish.Models;

namespace starskytest.FakeMocks;

public class FakeIRemotePublishService(bool publishEnabled = true) : IRemotePublishService
{
	public FtpPublishManifestModel? ManifestResult { get; set; } = new()
	{
		Slug = "test-slug", Copy = new Dictionary<string, bool> { { "test.jpg", true } }
	};

	public bool RunResult { get; set; } = true;
	public string? LastPath { get; set; }
	public string? LastProfileId { get; set; }
	public string? LastSlug { get; set; }

	public Task<FtpPublishManifestModel?> IsValidZipOrFolder(string inputFullFileDirectoryOrZip)
	{
		LastPath = inputFullFileDirectoryOrZip;
		return Task.FromResult(ManifestResult);
	}

	public bool Run(string parentDirectoryOrZipFile, string profileId, string slug,
		Dictionary<string, bool> copyContent)
	{
		LastPath = parentDirectoryOrZipFile;
		LastProfileId = profileId;
		LastSlug = slug;
		return RunResult;
	}

	public bool IsPublishEnabled(string publishProfileName)
	{
		return publishEnabled;
	}
}
