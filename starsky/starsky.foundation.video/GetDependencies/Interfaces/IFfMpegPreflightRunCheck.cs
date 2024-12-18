namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IFfMpegPreflightRunCheck
{
	Task<bool> TryRun();
	Task<bool> TryRun(string currentArchitecture);
}
