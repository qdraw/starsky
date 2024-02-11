using Microsoft.Extensions.Logging;

namespace starskydesktop;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddLogging(logging =>
		{
			logging.AddDebug();
			logging.AddConsole();
			logging.AddSimpleConsole();
		});

		var service = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<MainPage>>();
		service.LogInformation("test");
		
		RunBackgroundService.Run().ConfigureAwait(false);
			
		return builder.Build();
	}
}
