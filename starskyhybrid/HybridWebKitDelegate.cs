using WebKit;
using Foundation;
using System.Text.Json;
using System.Threading.Tasks;

namespace starskyhybrid
{
    public class HybridWebKitDelegate : NSObject, IWKScriptMessageHandler
    {
        public WKWebView WebView { get; set; }
        private readonly HybridFetchBridge _bridge;

        public HybridWebKitDelegate(HybridFetchBridge bridge)
        {
            _bridge = bridge;
        }

        [Export("userContentController:didReceiveScriptMessage:")]
        public async void DidReceiveScriptMessage(WKUserContentController controller, WKScriptMessage message)
        {
            var req = JsonSerializer.Deserialize<HybridFetchBridge.FetchRequest>(message.Body.ToString());
            var result = await _bridge.HandleApiCallAsync(req);
            var json = JsonSerializer.Serialize(result);
            var jsCallback = $"window.onNativeResponse({json});";
            await WebView.EvaluateJavaScriptAsync(new NSString(jsCallback));
        }
    }
}
