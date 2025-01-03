using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using AwsSignatureVersion4.Private;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace FiatChamp.Extensions;

public static class FlurlExtensions
{
    public static IFlurlRequest SignAws(this IFlurlRequest request, ImmutableCredentials credentials, RegionEndpoint regionEndpoint, object? data = null)
    {
        return request.BeforeCall(call =>
        {
            var json = data == null 
                ? string.Empty 
                : JsonSerializer.Serialize(data);
            call.HttpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
            Signer.Sign(call.HttpRequestMessage, null, [], DateTime.Now, regionEndpoint.SystemName, "execute-api", credentials);
        });
    }

    public static async Task<IFlurlResponse> SignAwsAndPostJsonAsync(this IFlurlRequest request, ImmutableCredentials credentials, RegionEndpoint regionEndpoint, object? data = null) =>
        await request
            .SignAws(credentials, regionEndpoint, data)
            .PostJsonAsync(data);

    public static async Task<T> DumpResponseAsync<T>(this Task<T> resultTask, ILogger logger)
    {
        var result = await resultTask;
        logger.LogDebug("Response: {dump}", result.Dump());
        return result;
    }
}