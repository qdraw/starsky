using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.realtime.Model;

namespace starsky.foundation.realtime.Services
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class HeartbeatService : IHostedService
	{
		private const int SpeedInSeconds = 30;

		private readonly IWebSocketConnectionsService _connectionsService;

		private Task _heartbeatTask;
		private CancellationTokenSource _cancellationTokenSource;

		public HeartbeatService(IWebSocketConnectionsService connectionsService)
		{
			_connectionsService = connectionsService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			_heartbeatTask = HeartbeatAsync(_cancellationTokenSource.Token);

			return _heartbeatTask.IsCompleted ? _heartbeatTask : Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			if (_heartbeatTask != null)
			{
				_cancellationTokenSource.Cancel();

				await Task.WhenAny(_heartbeatTask, Task.Delay(-1, cancellationToken));

				try
				{
					cancellationToken.ThrowIfCancellationRequested();
				}
				catch (OperationCanceledException e )
				{
					Console.WriteLine("catch-ed exception ->");
					Console.WriteLine(e);
				}
			}
		}

		private async Task HeartbeatAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var webSocketResponse =
					new ApiNotificationResponseModel<HeartbeatModel>(new HeartbeatModel(SpeedInSeconds), 
						ApiNotificationType.Heartbeat);
				await _connectionsService.SendToAllAsync(webSocketResponse, cancellationToken);
				await Task.Delay(TimeSpan.FromSeconds(SpeedInSeconds), cancellationToken);
			}
		}
	}
}
