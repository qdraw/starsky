using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Http;
using starsky.foundation.injection;

namespace starskycore.Helpers
{
	[Service(InjectionLifetime = InjectionLifetime.Scoped)]
	public class ApplicationInsightsJsHelper
	{
		private readonly IHttpContextAccessor _httpContext;
		private readonly JavaScriptSnippet _aiJavaScriptSnippet;
        
		/// <summary>
		/// Init script helper - need a csp-nonce in the context
		/// </summary>
		/// <param name="httpContext">Your IHttpContext</param>
		/// <param name="aiJavaScriptSnippet">the snip-it from app insights</param>
		public ApplicationInsightsJsHelper(IHttpContextAccessor httpContext, JavaScriptSnippet aiJavaScriptSnippet = null)
		{
			_httpContext = httpContext;
			if(aiJavaScriptSnippet != null) _aiJavaScriptSnippet = aiJavaScriptSnippet;
		}
		
		/// <summary>
		/// Get the App Insights front-end script with your app insights token visible
		/// Need a csp-nonce in the context
		/// </summary>
		public string ScriptTag
		{
			get
			{
				if ( _aiJavaScriptSnippet == null ) return "<!-- ApplicationInsights JavaScriptSnippet disabled -->";
				var js = _aiJavaScriptSnippet.FullScript;
				
				// Replace the default script with a nonce version, to avoid XSS attacks
				const string scriptTagStart = @"<script type=""text/javascript"">";
				var scriptTagStartWithNonce = "<script type=\"text/javascript\" " +
				                              $"nonce=\"{_httpContext.HttpContext.Items["csp-nonce"]}\">";
				var script = js.Replace(scriptTagStart, scriptTagStartWithNonce);
				return script;
			}
		}
		
		/// <summary>
		/// Get the App Insights front-end script with your app insights token visible
		/// Need a csp-nonce in the context
		/// </summary>
		public string ScriptPlain
		{
			get
			{
				if ( _aiJavaScriptSnippet == null ) return "/* ApplicationInsights JavaScriptSnippet disabled */";
				var js = _aiJavaScriptSnippet.FullScript;
				
				// Replace the default script with a nonce version, to avoid XSS attacks
				const string scriptTagStart = @"<script type=""text/javascript"">";
				const string scriptEndStart = @"</script>";

				var script = js.Replace(scriptTagStart, string.Empty);
				script = script.Replace(scriptEndStart, string.Empty);

				return script;
			}
		}
	}
}
