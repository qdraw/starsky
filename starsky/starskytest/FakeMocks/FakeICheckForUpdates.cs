using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.health.UpdateCheck.Interfaces;
using starsky.feature.health.UpdateCheck.Models;

namespace starskytest.FakeMocks
{
	public class FakeICheckForUpdates : ICheckForUpdates
	{
		private KeyValuePair<UpdateStatus, string> Status { get; set; }
		
		public FakeICheckForUpdates(KeyValuePair<UpdateStatus, string> status)
		{
			Status = status;
		}
		
#pragma warning disable 1998
		public async Task<KeyValuePair<UpdateStatus, string>> IsUpdateNeeded(string currentVersion = "")
#pragma warning restore 1998
		{
			return Status;
		}

	}
}
