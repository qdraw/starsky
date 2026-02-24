using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks;

public sealed class FakeImageOptimisationService : IImageOptimisationService
{
	public bool Called { get; private set; }
	public List<Optimizer>? ReceivedOptimizers { get; private set; }

	public Task Optimize(IReadOnlyCollection<ImageOptimisationItem> images,
		List<Optimizer>? optimizers = null)
	{
		Called = true;
		ReceivedOptimizers = optimizers;
		return Task.CompletedTask;
	}
}
