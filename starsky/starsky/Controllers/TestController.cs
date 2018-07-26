using Microsoft.AspNetCore.Mvc;
using starsky.Models;

namespace starsky.Controllers
{
    public class TestController : Controller
    {
        private readonly AppSettings _appSettings;
        public TestController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        
        public IActionResult Index()
        {
            return Json(_appSettings);
        }
    }
}