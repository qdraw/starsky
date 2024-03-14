using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.health.UpdateCheck.Interfaces;
using starsky.feature.health.UpdateCheck.Services;

namespace starskytest.FakeMocks;

public class FakeISpecificVersionReleaseInfo : ISpecificVersionReleaseInfo
{
	private readonly Dictionary<string, Dictionary<string, string>>? _releaseInfo;

	public FakeISpecificVersionReleaseInfo(
		Dictionary<string, Dictionary<string, string>>? releaseInfo = null)
	{
		_releaseInfo = releaseInfo;
	}

	public Task<string> SpecificVersionMessage(string? versionToCheckFor)
	{
		versionToCheckFor ??= string.Empty;
		if ( _releaseInfo?.TryGetValue(versionToCheckFor, out var valueDict) is not true )
			return Task.FromResult(string.Empty);

		var value = valueDict.TryGetValue("en", out var languageValue)
			? SpecificVersionReleaseInfo.ConvertMarkdownLinkToHtml(languageValue)
			: string.Empty;
		return Task.FromResult(value);
	}
}
