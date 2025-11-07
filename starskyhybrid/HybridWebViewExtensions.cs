using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;

namespace starskyhybrid
{
    public static class HybridWebViewExtensions
    {
        public static async Task EnableFetchBridgeAsync(this WebView webView, HybridFetchBridge bridge)
        {
            webView.Navigated += async (s, e) =>
            {
                var js = File.ReadAllText("clientapp/public/inject-fetch-bridge.js");
                await webView.EvaluateJavaScriptAsync(js);
            };

            webView.WebMessageReceived += async (s, e) =>
            {
                var req = System.Text.Json.JsonSerializer.Deserialize<HybridFetchBridge.FetchRequest>(e.Message);
                var result = await bridge.HandleApiCallAsync(req);
                await webView.PostWebMessageAsJsonAsync(result);
            };
        }
    }
}
