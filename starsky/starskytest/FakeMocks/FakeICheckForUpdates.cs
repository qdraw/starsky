using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.health.UpdateCheck.Interfaces;
using starsky.feature.health.UpdateCheck.Models;

namespace starskytest.FakeMocks;

public class FakeICheckForUpdates : ICheckForUpdates
{
	public FakeICheckForUpdates(KeyValuePair<UpdateStatus, string?> status)
	{
		Status = status;
	}

	private KeyValuePair<UpdateStatus, string?> Status { get; }

	public Task<(UpdateStatus, string?)> IsUpdateNeeded(string currentVersion = "")
	{
		return Task.FromResult(( Status.Key, Status.Value ));
	}
}
