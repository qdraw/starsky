using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.optimisation.Interfaces;

public interface IImageOptimisationService
{
	Task Optimize(ImageOptimisationItem image,
		List<Optimizer>? optimizers = null);

	Task Optimize(IReadOnlyCollection<ImageOptimisationItem> images,
		List<Optimizer>? optimizers = null);
}
