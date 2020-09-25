using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text.Json;
using ChannelWSClient.Model;

namespace ChannelWSClient
{
    public class Program
    {
        public const int KEYSTROKE_TRANSMIT_INTERVAL_MS = 100;
        public const int CLOSE_SOCKET_TIMEOUT_MS = 10000;

        // async Main requires C# 7.2 or newer in csproj properties
        static async Task Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                await MainThreadUiLoop(ReadSettings());
                Console.WriteLine("\nPress R to re-connect or any other key to exit.");
                var key = Console.ReadKey(intercept: true);
                running = (key.Key == ConsoleKey.R);
            }
        }

        private static AppSettings ReadSettings()
        {
	        var appSettingsFile = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.json");
	        if ( File.Exists(appSettingsFile) )
	        {
		        return ParseAppSettings(appSettingsFile);
	        }
	        var appSettingsPatchFile = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.patch.json");
	        if ( !File.Exists(appSettingsPatchFile) )
		        throw new FileNotFoundException("missing appSettings file");
	        return ParseAppSettings(appSettingsPatchFile);
        }

        private static AppSettings ParseAppSettings(string appSettingsFile)
        {
	        var jsonAsString = File.ReadAllText(appSettingsFile);
	        var settings=  JsonSerializer.Deserialize<AppSettings>(jsonAsString);
	        if ( string.IsNullOrEmpty(settings.Url) )
	        {
		        throw new ArgumentNullException("Should enter username in appSettings");
	        }
	        if ( string.IsNullOrEmpty(settings.Password) || string.IsNullOrEmpty(settings.Username))
	        {
		        Console.WriteLine("WARNING  < Missing username or password");
	        }
	        return settings;
        }

        static async Task MainThreadUiLoop(AppSettings appSettings)
        {
            try
            {
                await WebSocketClient.StartAsync(appSettings);
                Console.WriteLine("Press ESC to exit. Other keystrokes are sent to the echo server.\n\n");
                bool running = true;
                while (running && WebSocketClient.State == WebSocketState.Open)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            running = false;
                        }
                        else
                        {
                            WebSocketClient.QueueKeystroke(key.KeyChar.ToString());
                        }
                    }
                }
                await WebSocketClient.StopAsync();
            }
            catch (OperationCanceledException)
            {
                // normal upon task/token cancellation, disregard
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public static void ReportException(Exception ex, [CallerMemberName] string location = "(Caller name not set)")
        {
            Console.WriteLine($"\n{location}:\n  Exception {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"  Inner Exception {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
        }
    }
}
