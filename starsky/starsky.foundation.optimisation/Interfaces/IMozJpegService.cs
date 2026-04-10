using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.optimisation.Interfaces;

public interface IMozJpegService
{
	Task RunMozJpeg(Optimizer optimizer,
		IEnumerable<ImageOptimisationItem> targets);
}
