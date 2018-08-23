using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

////
//    Here is a sample code that only depends on Razor (for parsing and C# code generation)
//     and Roslyn (for C# code compilation, but you could use the old CodeDom as well).
//    There is no MVC in that piece of code, so, no View, no .cshtml files, no Controller,
// just Razor source parsing and compiled runtime execution. There is still the notion of Model though.
//    You will only need to add following nuget packages: Microsoft.AspNetCore.Razor.Language (v2.1.1),
// Microsoft.AspNetCore.Razor.Runtime (v2.1.1) and Microsoft.CodeAnalysis.CSharp (v2.8.2) nugets.
//    //


namespace starsky.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName);
        Task<string> RenderToStringAsync<TModel>(string viewName, TModel model);
        string RenderToString<TModel>(string viewPath, TModel model);
        string RenderToString(string viewPath);
    }
     
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
     
        public ViewRenderService(IRazorViewEngine viewEngine, 
            IHttpContextAccessor httpContextAccessor,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _httpContextAccessor = httpContextAccessor;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }
     
        public string RenderToString<TModel>(string viewPath, TModel model)
        {
            try
            {
                var viewEngineResult = _viewEngine.GetView("~/", viewPath, false);
     
                if (!viewEngineResult.Success)
                {
                    throw new InvalidOperationException($"Couldn't find view {viewPath}");
                }
     
                var view = viewEngineResult.View;
     
                using (var sw = new StringWriter())
                {
                    var viewContext = new ViewContext()
                    {
                        HttpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider },
                        ViewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
                        Writer = sw
                    };
                    view.RenderAsync(viewContext).GetAwaiter().GetResult();
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error ending email.", ex);
            }
        }
     
        public async Task<string> RenderToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
     
            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(actionContext, viewName, false);
     
                // Fallback - the above seems to consistently return null when using the EmbeddedFileProvider
                if (viewResult.View == null)
                {
                    viewResult = _viewEngine.GetView("~/", viewName, false);
                }
     
                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }
     
                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };
     
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );
     
                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
     
        public string RenderToString(string viewPath)
        {
            return RenderToString(viewPath, string.Empty);
        }
     
        public Task<string> RenderToStringAsync(string viewName)
        {
            return RenderToStringAsync<string>(viewName, string.Empty);
        }
    }

}