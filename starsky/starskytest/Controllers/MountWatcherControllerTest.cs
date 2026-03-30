using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;

namespace starskytest.Controllers;

[TestClass]
public class MountWatcherControllerTest
{
	[TestMethod]
	public async Task StartStopStatus_ReturnsOk()
	{
		var service = new FakeCameraMountWatcherService();
		var sut = new MountWatcherController(service);

		var status = sut.Status() as OkObjectResult;
		Assert.IsNotNull(status);

		var start = await sut.StartAsync() as OkObjectResult;
		Assert.IsNotNull(start);

		var stop = await sut.StopAsync() as OkObjectResult;
		Assert.IsNotNull(stop);
	}

	[TestMethod]
	public async Task StartStop_AreIdempotentFromController()
	{
		var service = new FakeCameraMountWatcherService();
		var sut = new MountWatcherController(service);

		var start1 = await sut.StartAsync() as OkObjectResult;
		var start2 = await sut.StartAsync() as OkObjectResult;
		Assert.IsNotNull(start1);
		Assert.IsNotNull(start2);

		var stop1 = await sut.StopAsync() as OkObjectResult;
		var stop2 = await sut.StopAsync() as OkObjectResult;
		Assert.IsNotNull(stop1);
		Assert.IsNotNull(stop2);
	}

	private sealed class FakeCameraMountWatcherService : ICameraMountWatcherService
	{
		private readonly MountWatcherStatusModel _status = new();

		public MountWatcherStatusModel GetStatus()
		{
			return _status;
		}

		public Task<MountWatcherStatusModel> StartAsync()
		{
			_status.Running = true;
			return Task.FromResult(_status);
		}

		public Task<MountWatcherStatusModel> StopAsync()
		{
			_status.Running = false;
			return Task.FromResult(_status);
		}
	}
}


