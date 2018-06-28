using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Interfaces;

namespace starsky.Controllers
{
    public class ImportController : Controller
    {
        private readonly IImport _import;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ImportController(IImport import, IHostingEnvironment hostingEnvironment)
        {
            _import = import;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [HttpHead]
        public IActionResult Index1()
        {
            return NotFound();
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> Index()
        {
            // if (!IsApikeyValid(Request)) return BadRequest("Authorisation Error");

            var guid = DateTime.UtcNow.ToString("yyyyddMM_HHmmss__") + Guid.NewGuid().ToString().Substring(0, 20) + ".jpg";
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", guid);
            Console.WriteLine(path);
            using (var stream = System.IO.File.Create(path))
            {
                await Request.StreamFile(stream);
            }

            return Json(path);
        }
        
    }
}