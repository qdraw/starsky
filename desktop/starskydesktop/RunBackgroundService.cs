using static Medallion.Shell.Shell;
using Microsoft.Extensions.Hosting;
namespace starskydesktop;

public class RunBackgroundService : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Run();
	}
	
	private static readonly MemoryStream MemoryStream = new MemoryStream();

	public static async Task Run()
	{
		Console.WriteLine("RunBackgroundService");
		var command = Default.Run("/data/git/starsky/starsky/osx-arm64/starsky");
		await command.Task;
	}
}
