using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IImport _import;
        private readonly AppSettings _appSettings;

        public ImportController(IImport import, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _import = import;
        }

        [HttpGet]
        [ActionName("Index")]
        public IActionResult Index()
        {
            return View("Index");
        }

        
        [HttpPost]
        [ActionName("Index")]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(160000000)] // in bytes, 160mb
        public async Task<IActionResult> IndexPost()
        {
            var tempImportPaths = await Request.StreamFile(_appSettings);
            
            var importSettings = new ImportSettingsModel(Request);

            var importedFiles = _import.Import(tempImportPaths, importSettings);

            foreach (var path in tempImportPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                } 
            }
            
            if(importedFiles.Count == 0) Response.StatusCode = 206;
            
            return Json(importedFiles);
        }
    }
}