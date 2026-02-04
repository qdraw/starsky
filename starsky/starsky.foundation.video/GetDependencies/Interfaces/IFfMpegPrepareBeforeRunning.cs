namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IFfMpegPrepareBeforeRunning
{
	Task<bool> PrepareBeforeRunning(string currentArchitecture);
}
