using System;
using System.IO;
using System.Threading.Tasks;
using RazorLight;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskycore.Helpers;
using starskycore.Models;

namespace starskywebhtmlcli.Services
{
    public class ParseRazor
    {
        private readonly RazorLightEngine _engine;
        private readonly IStorage _hostFileSystemStorage;

        public ParseRazor(IStorage fileSystemStorage)
        {
	        _hostFileSystemStorage = fileSystemStorage;
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(starsky.feature.webhtmlpublish.Helpers.PublishManifest))
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
        
        public async Task<string> EmbeddedViews(string viewName, object viewModel)
        {
	        
            if (!_hostFileSystemStorage.ExistFile(new EmbeddedViewsPath().GetViewFullPath(viewName)))
            {
                Console.WriteLine("View Not Exist " + new EmbeddedViewsPath().GetViewFullPath(viewName));
            }
            else 
            {
	            // has an dependency on the filesystem by _engine.CompileRenderAsync
                return await 
                    _engine.CompileRenderAsync("WebHtmlPublish/EmbeddedViews/" + viewName, viewModel);
            }
            return string.Empty;
        }
    }
}
