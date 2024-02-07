using System;
using Microsoft.AspNetCore.Mvc;

namespace starsky.Controllers;

public class TestController : Controller
{
	public IActionResult Index()
	{
		return Content("Test");
	}
}
