using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace FiatChamp.Infrastructure;

public class PollyRequestHandler : DelegatingHandler
{
    private readonly ILogger<PollyRequestHandler> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;

    public PollyRequestHandler(ILogger<PollyRequestHandler> logger)
    {
        _logger = logger;
        _policy = Policy
            .HandleResult<HttpResponseMessage>(m => !m.IsSuccessStatusCode)
            .Or<HttpRequestException>(_ => true)
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i * 2), OnRetry);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => 
        await _policy.ExecuteAsync((_, ct) => base.SendAsync(request, ct), new Context { { "RequestUrl", request.RequestUri?.ToString() } }, cancellationToken);

    private void OnRetry(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan retryAfter, int retryCount, Context context)
    {
        var requestUrl = context["RequestUrl"];
        var exception = delegateResult.Exception as HttpRequestException;
        var reason = delegateResult.Result?.StatusCode.ToString() ??
                     exception?.StatusCode?.ToString() ??
                     exception?.Message;
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug(exception, "Error connecting to {url}. Reason: {reason}. Retrying in {retry}", requestUrl, reason, retryAfter);
        else
            _logger.LogWarning("Error connecting to {url}. Reason: {reason}. Retrying in {retry}", requestUrl, reason, retryAfter);
    }
}