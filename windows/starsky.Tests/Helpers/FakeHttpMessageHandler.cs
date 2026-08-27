using System.Net.Http;

namespace starsky.Tests.Helpers;

internal sealed class FakeHttpMessageHandler(params HttpResponseMessage[] responses)
	: HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new(responses);

    public FakeHttpMessageHandler(HttpStatusCode status, string content = "")
        : this(new HttpResponseMessage(status) { Content = new StringContent(content) }) { }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_responses.Count > 0 ? _responses.Dequeue() : new HttpResponseMessage(HttpStatusCode.OK));
}
