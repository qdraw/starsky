using System.Runtime.InteropServices;

namespace starsky.feature.externaldependencies.Interfaces;

public interface IExternalDependenciesService
{
	Task SetupAsync(OSPlatform? currentPlatform = null, Architecture? architecture = null);
}
