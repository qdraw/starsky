using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.health.UpdateCheck.Models;

namespace starsky.feature.health.UpdateCheck.Interfaces
{
	public interface ICheckForUpdates
	{
		Task<KeyValuePair<UpdateStatus, string>> IsUpdateNeeded();
	}
}
