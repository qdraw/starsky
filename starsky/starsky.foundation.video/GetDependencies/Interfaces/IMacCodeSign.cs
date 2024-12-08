namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IMacCodeSign
{
	Task<bool> MacCodeSignAndXattrExecutable(string exeFile);
}
