using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.Tests.Helpers;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses;

    public FakeHttpMessageHandler(HttpStatusCode status, string content = "")
        : this(new HttpResponseMessage(status) { Content = new StringContent(content) }) { }

    public FakeHttpMessageHandler(params HttpResponseMessage[] responses)
        => _responses = new Queue<HttpResponseMessage>(responses);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage _, CancellationToken __)
        => Task.FromResult(_responses.Count > 0 ? _responses.Dequeue() : new HttpResponseMessage(HttpStatusCode.OK));
}
