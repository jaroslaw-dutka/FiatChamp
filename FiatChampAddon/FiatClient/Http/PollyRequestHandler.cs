using Microsoft.Extensions.Logging;
using Polly;

namespace FiatChamp.Http;

public class PollyRequestHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public PollyRequestHandler(ILogger logger, HttpClientHandler innerHandler)
    {
        _logger = logger;
        InnerHandler = innerHandler;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(m => !m.IsSuccessStatusCode)
            .Or<HttpRequestException>(e => true)
            .WaitAndRetryAsync([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)],
                (delegateResult, time, retryCount, ctx) =>
                {
                    var ex = delegateResult.Exception as HttpRequestException;
                    var result = delegateResult.Result?.StatusCode.ToString() ?? ex?.StatusCode.ToString() ?? ex?.Message;
                    _logger.LogWarning("Error connecting to {0}. Result: {1}. Retrying in {2}", request.RequestUri, result, time);
                });

        return retryPolicy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
    }
}