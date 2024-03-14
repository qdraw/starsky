using System.Threading.Tasks;

namespace starsky.feature.health.UpdateCheck.Interfaces;

public interface ISpecificVersionReleaseInfo
{
	Task<string> SpecificVersionMessage(string? versionToCheckFor);
}
