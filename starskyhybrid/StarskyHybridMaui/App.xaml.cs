using Microsoft.Maui.Controls;

namespace StarskyHybridMaui
{
    public partial class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
