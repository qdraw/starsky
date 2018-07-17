using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Interfaces;

namespace starsky.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IImport _import;
        
        public ImportController(IImport import)
        {
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
        public async Task<IActionResult> IndexPost()
        {
            var tempImportPaths = await Request.StreamFile();

            var importedFiles = _import.Import(tempImportPaths, true);

            foreach (var path in tempImportPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                } 
            }
            
            if(importedFiles.Count == 0) Response.StatusCode = 409;
            
            return Json(importedFiles);
        }
    }
}