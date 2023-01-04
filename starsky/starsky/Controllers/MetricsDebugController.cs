using Microsoft.AspNetCore.Mvc;
using starsky.foundation.worker.CpuEventListener.Interfaces;
using starskycore.ViewModels;

namespace starsky.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class MetricsDebugController : Controller
{
	private readonly ICpuUsageListener _cpuUsageListener;

	public MetricsDebugController(ICpuUsageListener cpuUsageListener)
	{
		_cpuUsageListener = cpuUsageListener;
	}

	public IActionResult Index()
	{
		return Json(new MetricsDebugViewModel
		{
			CpuUsageMean = _cpuUsageListener.CpuUsageMean,
		});
	}
}
