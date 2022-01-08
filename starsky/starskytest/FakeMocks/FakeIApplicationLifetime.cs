using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace starskytest.FakeMocks
{
	public class FakeIApplicationLifetime : IHostApplicationLifetime
	{
		public void StopApplication()
		{
			throw new NotImplementedException();
		}

		public CancellationToken ApplicationStarted { get; }
		public CancellationToken ApplicationStopped { get; }
		public CancellationToken ApplicationStopping { get; }
	}
}

