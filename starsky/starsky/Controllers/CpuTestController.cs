using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace starsky.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class CpuTestController : Controller
{
	public async Task<IActionResult> Index()
	{
		var cpu = await GetCpuUsageForProcess();
		return Ok(cpu.ToString(CultureInfo.InvariantCulture));
	}
	
	private static async Task<double> GetCpuUsageForProcess()
	{
		var startTime = DateTime.UtcNow;
		var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

		await Task.Delay(500);

		var endTime = DateTime.UtcNow;
		var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

		var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
		var totalMsPassed = (endTime - startTime).TotalMilliseconds;

		var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

		return cpuUsageTotal * 100;
	}
}
