using Microsoft.AspNetCore.Mvc;
using starsky.foundation.worker.CpuEventListener.Interfaces;
using starskycore.ViewModels;

namespace starsky.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class MetricsDebugController : Controller
{
	private readonly ICpuUsageListenerBackgroundService _cpuUsageListenerBackgroundService;

	public MetricsDebugController(ICpuUsageListenerBackgroundService cpuUsageListenerBackgroundService)
	{
		_cpuUsageListenerBackgroundService = cpuUsageListenerBackgroundService;
	}

	public IActionResult Index()
	{
		return Json(new MetricsDebugViewModel
		{
			CpuUsageMean = _cpuUsageListenerBackgroundService.CpuUsageMean,
		});
	}
}
