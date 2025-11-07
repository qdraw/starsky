using Microsoft.Maui.Controls;
using starskyhybrid;
using System.IO;

namespace StarskyHybridMaui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            var webView = new WebView
            {
                Source = new HtmlWebViewSource
                {
                    Html = File.ReadAllText("/Users/dion/data/git/starsky/clientapp/public/index.html")
                },
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var bridge = new HybridFetchBridge();
            webView.EnableFetchBridgeAsync(bridge).Wait();

            Content = webView;
        }
    }
}
