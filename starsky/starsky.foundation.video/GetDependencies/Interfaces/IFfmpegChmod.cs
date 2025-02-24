namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IFfmpegChmod
{
	Task<bool> Chmod(string exeFile);
}
