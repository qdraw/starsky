using System;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.sync.Interfaces
{
	public interface IAutoResetEventAsync
	{
		Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
		Task<bool> WaitAsync(TimeSpan timeout);
		void Set();
		string ToString();
	}
}
