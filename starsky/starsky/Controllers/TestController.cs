using System;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.diagnosticsource.Metrics;

namespace starsky.Controllers;

public class TestController : Controller
{
	private readonly HatCoMetrics _hatCoMetrics;

	public TestController(HatCoMetrics hatCoMetrics)
	{
		_hatCoMetrics = hatCoMetrics;
	}

	public IActionResult Index()
	{
		_hatCoMetrics.HatsSold(new Random().Next(100));
		return Content("Test");
	}
}
