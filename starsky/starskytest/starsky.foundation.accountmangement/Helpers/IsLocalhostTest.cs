using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Helpers;

namespace starskytest.starsky.foundation.accountmangement.Helpers
{
	[TestClass]
	public class IsLocalhostTest
	{
		[TestMethod]
		public void Null()
		{
			Assert.IsFalse(IsLocalhost.IsHostLocalHost(null,null));
		}
		
		[TestMethod]
		public void OnOfBothNull()
		{
			Assert.IsFalse(IsLocalhost.IsHostLocalHost(null,IPAddress.Loopback));
		}
				
		[TestMethod]
		public void IpAddressLoopback()
		{
			Assert.IsTrue(IsLocalhost.IsHostLocalHost(IPAddress.Loopback,IPAddress.Loopback));
		}
		
		[TestMethod]
		public void IpAddressIPv6Loopback()
		{
			Assert.IsTrue(IsLocalhost.IsHostLocalHost(IPAddress.IPv6Loopback,IPAddress.IPv6Loopback));
		}
						
		[TestMethod]
		public void FromRemote()
		{
			Assert.IsFalse(IsLocalhost.IsHostLocalHost(IPAddress.Loopback,IPAddress.Parse("8.8.8.8")));
		}
	}
}
