using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace starskycore.Helpers
{
	public class ApplicationInsightsJsHelper
	{
		private readonly IHttpContextAccessor _httpContext;
		private readonly JavaScriptSnippet _aiJavaScriptSnippet;
        
		public ApplicationInsightsJsHelper(IHttpContextAccessor httpContext, JavaScriptSnippet aiJavaScriptSnippet = null)
		{
			_httpContext = httpContext;
			if(aiJavaScriptSnippet != null) _aiJavaScriptSnippet = aiJavaScriptSnippet;
		}
		public string Script
		{
			get
			{
				if ( _aiJavaScriptSnippet == null ) return "<!-- ApplicationInsights JavaScriptSnippet disabled -->";
				var js = _aiJavaScriptSnippet.FullScript;
				const string scriptTagStart = @"<script type=""text/javascript"">";
				var scriptTagStartWithNonce = $"<script type=\"text/javascript\" nonce=\"{_httpContext.HttpContext.Items["csp-nonce"]}\">";
				var script = js.Replace(scriptTagStart, scriptTagStartWithNonce);
				return script;
			}
		}
	}
}
