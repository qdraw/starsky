using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.worker.CpuEventListener;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.CpuEventListener;

[TestClass]
public class CpuUsageListenerBackgroundServiceTest
{
	[TestMethod]
	public async Task CpuUsageListenerBackgroundService1_Start()
	{
		var service = new CpuUsageListenerBackgroundService(new FakeIWebLogger());
		await service.StartAsync(new CancellationToken(true));
		Assert.AreEqual(0,service.LastValue);
	}
	
	[TestMethod]
	public async Task CpuUsageListenerBackgroundService1_Stop_Nullable()
	{
		var service = new CpuUsageListenerBackgroundService(new FakeIWebLogger());
		await service.StopAsync(new CancellationToken(true));
		Assert.AreEqual(0,service.LastValue);
	}
	
	[TestMethod]
	public async Task CpuUsageListenerBackgroundService1_StartStop()
	{
		var service = new CpuUsageListenerBackgroundService(new FakeIWebLogger());
		await service.StartAsync(new CancellationToken(true));
		await service.StopAsync(new CancellationToken(true));
		Assert.AreEqual(0,service.LastValue);
	}
	
	[TestMethod]
	public void CpuUsageListenerBackgroundService1_Nullable()
	{
		var service = new CpuUsageListenerBackgroundService(new FakeIWebLogger());
		Assert.AreEqual(0,service.LastValue);
	}
}
