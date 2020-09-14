using System;
using System.Threading.Tasks;
using starsky.foundation.sockets.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeISockets : ISockets
	{
#pragma warning disable 1998
		public  async Task BroadcastAll(Guid? requestId, object message)
#pragma warning restore 1998
		{
			
		}
	}
}
