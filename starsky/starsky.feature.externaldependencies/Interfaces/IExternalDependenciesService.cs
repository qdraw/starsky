namespace starsky.feature.externaldependencies.Interfaces;

public interface IExternalDependenciesService
{
	Task SetupAsync(string[] args);
}
