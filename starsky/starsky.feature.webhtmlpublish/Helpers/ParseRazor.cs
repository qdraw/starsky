using System;
using System.Threading.Tasks;
using RazorLight;
using starsky.feature.webhtmlpublish.ViewModels;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webhtmlpublish.Helpers
{
    public class ParseRazor
    {
        private readonly RazorLightEngine _engine;
        private readonly IStorage _hostFileSystemStorage;
        private readonly IWebLogger _logger;

        public ParseRazor(IStorage fileSystemStorage, IWebLogger logger)
        {
	        _hostFileSystemStorage = fileSystemStorage;
	        _logger = logger;
	        _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(PublishManifest))
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

        public bool Exist(string viewName)
        {
	        var path = EmbeddedViewsPath.GetViewFullPath(viewName);
	        return _hostFileSystemStorage.ExistFile(path);
        }
        
        public async Task<string> EmbeddedViews(string viewName, object viewModel)
        {
	        if ( Exist(viewName) )
	        {
		        // has an dependency on the filesystem by _engine.CompileRenderAsync
		        return await
			        _engine.CompileRenderAsync(
				        "WebHtmlPublish/EmbeddedViews/" + viewName, viewModel);
	        }
	        
	        _logger.LogInformation("View Not Exist " + EmbeddedViewsPath.GetViewFullPath(viewName));
	        return string.Empty;
        }
    }
}
