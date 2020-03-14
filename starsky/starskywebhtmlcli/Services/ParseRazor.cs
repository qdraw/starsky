using System;
using System.IO;
using System.Threading.Tasks;
using RazorLight;
using starsky.foundation.database.Models;
using starskycore.Helpers;
using starskycore.Models;

namespace starskywebhtmlcli.Services
{
    public class ParseRazor
    {
        private readonly RazorLightEngine _engine;

        public ParseRazor()
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Program))
				.UseEmbeddedResourcesProject(typeof(ViewModels.WebHtmlViewModel))
				.UseEmbeddedResourcesProject(typeof(AppSettings))
				.UseEmbeddedResourcesProject(typeof(FileIndexItem))
                .UseEmbeddedResourcesProject(typeof(System.Linq.Enumerable))
                .UseEmbeddedResourcesProject(typeof(AppSettingsPublishProfiles))
                .UseFileSystemProject(AppDomain.CurrentDomain.BaseDirectory )
                        // > starskywebhtmlcli/bin folder
                .UseMemoryCachingProvider()
                .Build();
        }

        public string GetViewFullpath(string viewName)
        {
            return AppDomain.CurrentDomain.BaseDirectory +
                   Path.DirectorySeparatorChar +
                   "EmbeddedViews" +
                   Path.DirectorySeparatorChar + viewName;
        }
        
        public async Task<string> EmbeddedViews(string viewName, object viewModel)
        {
            if (FilesHelper.IsFolderOrFile(GetViewFullpath(viewName)) 
                == FolderOrFileModel.FolderOrFileTypeList.Deleted)
            {
                Console.WriteLine("View Not Exist " + GetViewFullpath(viewName));
            }
            else if (FilesHelper.IsFolderOrFile(GetViewFullpath(viewName)) 
                     == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                return await 
                    _engine.CompileRenderAsync("EmbeddedViews/" + viewName, viewModel);
            }
            return string.Empty;
        }
    }
}
