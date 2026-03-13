using System.Text.Json;
using starsky.feature.trash.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.trash.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MoveToTrashJobHandler(
	IMoveToTrashService moveToTrashService) : IBackgroundJobHandler
{
	public const string MoveToTrash = "Trash.MoveToTrash.v1";

	public string JobType => MoveToTrash;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<MoveToTrashPayload>(payloadJson) ??
		              throw new ArgumentException("Invalid payload", nameof(payloadJson));

		await moveToTrashService.MoveToTrashAsync(payload);
	}
}
