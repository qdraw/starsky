using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sentry.Extensibility;
using Sentry.Protocol.Envelopes;

namespace starskytest.FakeMocks;

public sealed class FakeSentryTransport : ITransport
{
	public List<Envelope> Envelopes { get; } = [];

	public Task SendEnvelopeAsync(Envelope envelope,
		CancellationToken cancellationToken = default)
	{
		Envelopes.Add(envelope);
		return Task.CompletedTask;
	}
}
