using System;
using System.Threading.Tasks;
using RazorLight;
using starsky.feature.webhtmlpublish.ViewModels;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webhtmlpublish.Helpers
{
    public class ParseRazor
    {
        private readonly RazorLightEngine _engine;
        private readonly IStorage _hostFileSystemStorage;

        public ParseRazor(IStorage fileSystemStorage)
        {
	        _hostFileSystemStorage = fileSystemStorage;
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Helpers.PublishManifest))
				.UseEmbeddedResourcesProject(typeof(WebHtmlViewModel))
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
