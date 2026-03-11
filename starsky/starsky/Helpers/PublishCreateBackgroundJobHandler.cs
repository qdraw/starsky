using System.Threading;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.Helpers;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class PublishCreateBackgroundJobHandler(
	PublishCreateBackgroundJobRunner runner) : IBackgroundJobHandler
{
	public const string JobTypeValue = PublishCreateBackgroundJobRunner.JobTypeValue;
	public string JobType => JobTypeValue;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		await runner.ExecuteAsync(payloadJson);
	}
}
