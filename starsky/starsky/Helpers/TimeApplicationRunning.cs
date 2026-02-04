using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.packagetelemetry.Interfaces;

namespace starsky.Helpers;

public static class TimeApplicationRunning
{
	public static void SetRunningTime(IApplicationBuilder app, DateTime startTime)
	{
		var lifetime = app.ApplicationServices
			.GetRequiredService<IHostApplicationLifetime>();

		lifetime.ApplicationStopping.Register(() =>
		{
			using var scope = app.ApplicationServices.CreateScope();
			var diagnostics = scope.ServiceProvider
				.GetRequiredService<ILifetimeDiagnosticsService>();
			diagnostics.AddOrUpdateApplicationStopping(startTime);
		});
	}
}
