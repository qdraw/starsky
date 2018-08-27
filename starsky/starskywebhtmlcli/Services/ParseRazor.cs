using System;
using System.IO;
using System.Threading.Tasks;
using RazorLight;
using starsky.Helpers;
using starsky.Models;

namespace starskywebhtmlcli.Services
{
    public class ParseRazor
    {
        private RazorLightEngine _engine;

        public ParseRazor()
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(starsky.Program))
                .UseFilesystemProject(AppDomain.CurrentDomain.BaseDirectory )
                        // > starskywebhtmlcli/bin folder
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> EmbeddedViews(string viewName, object viewModel)
        {
            var viewPath = AppDomain.CurrentDomain.BaseDirectory +
                           Path.DirectorySeparatorChar +
                           "EmbeddedViews" +
                           Path.DirectorySeparatorChar + viewName;

            if (Files.IsFolderOrFile(viewPath) 
                == FolderOrFileModel.FolderOrFileTypeList.Deleted)
            {
                Console.WriteLine("View Not Exist");
            }
            else if (Files.IsFolderOrFile(viewPath) 
                     == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                return await 
                    _engine.CompileRenderAsync("EmbeddedViews/" + viewName, viewModel);
            }
            return string.Empty;
        }
    }
}