namespace starsky.foundation.optimisation.Interfaces;

public interface IImageOptimisationChmod
{
	Task<bool> Chmod(string exeFile);
}
