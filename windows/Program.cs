using System.Diagnostics.CodeAnalysis;
using Velopack;

namespace Starsky.Desktop;

[ExcludeFromCodeCoverage]
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
