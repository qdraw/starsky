using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Controllers
{
    public class ImportController : Controller
    {
        private readonly IImport _import;

        
        public ImportController(IImport import)
        {
            _import = import;
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

            var path = GetTempFilePath();
            using (var stream = System.IO.File.Create(path))
            {
                // In mstest is has no Request item
                try
                {
                    await Request.StreamFile(stream);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e);
                }
            }

            _import.Import(path, true);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            
            return Json(path);
        }

        public string GetTempFilePath()
        {
            var guid = DateTime.UtcNow.ToString("yyyyddMM_HHmmss__") + Guid.NewGuid().ToString().Substring(0, 20) + ".jpg";
            var path = Path.Combine(AppSettingsProvider.ThumbnailTempFolder, guid);
            return path;
        }
            
        
    }
}