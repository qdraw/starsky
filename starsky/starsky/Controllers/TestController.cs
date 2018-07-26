using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using starsky.Models;

namespace starsky.Controllers
{
    public class TestController : Controller
    {
        private readonly IConfiguration _iconfig;
        public TestController(IConfiguration config)
        {
            _iconfig = config;
        }
        public IActionResult Index()
        {
            var t = _iconfig.GetSection("App") as AppSettings;
            return Json(t);
        }
    }
}