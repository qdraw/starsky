using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.realtime.Interfaces;

namespace starsky.foundation.realtime.Services
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class HeartbeatService : IHostedService
	{
		#region Fields

		private const int SpeedInSeconds = 30;
		private const string InsertDateToken = "INSERT_DATE_TOKEN";
		private const string SpeedInSecondsToken = "SPEED_TOKEN";
		private const string HeartbeatMessage = "{ \"speed\": " + SpeedInSecondsToken + ",  \"time\": \"" + InsertDateToken + "\"} ";

		private readonly IWebSocketConnectionsService _webSocketConnectionsService;

		private Task _heartbeatTask;
		private CancellationTokenSource _cancellationTokenSource;
		#endregion

		#region Constructor
		public HeartbeatService(IWebSocketConnectionsService webSocketConnectionsService)
		{
			_webSocketConnectionsService = webSocketConnectionsService;
		}
		#endregion

		#region Methods
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

				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		private async Task HeartbeatAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var message = HeartbeatMessage.Replace(SpeedInSecondsToken, SpeedInSeconds.ToString()).Replace(
					InsertDateToken, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
				await _webSocketConnectionsService.SendToAllAsync(message, cancellationToken);
				await Task.Delay(TimeSpan.FromSeconds(SpeedInSeconds), cancellationToken);
			}
		}
		#endregion
	}
}
