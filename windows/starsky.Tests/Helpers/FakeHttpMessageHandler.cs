using System.Net.Http;

namespace starsky.Tests.Helpers;

internal sealed class FakeHttpMessageHandler(params HttpResponseMessage[] responses)
	: HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new(responses);
    private HttpResponseMessage? _last;

    public FakeHttpMessageHandler(HttpStatusCode status, string content = "")
        : this(new HttpResponseMessage(status) { Content = new StringContent(content) }) { }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_responses.Count > 0)
        {
	        _last = _responses.Dequeue();
        }

        return Task.FromResult(_last ?? new HttpResponseMessage(HttpStatusCode.OK));
    }
}
