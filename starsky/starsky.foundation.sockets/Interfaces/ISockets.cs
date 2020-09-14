using System;
using System.Threading.Tasks;

namespace starsky.foundation.sockets.Interfaces
{
	public interface ISockets
	{
		Task BroadcastAll(Guid? requestId, object message);
	}
}
