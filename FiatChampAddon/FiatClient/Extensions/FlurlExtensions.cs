using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using AwsSignatureVersion4.Private;
using Flurl.Http;

namespace FiatChamp.Extensions;

public static class FlurlExtensions
{
    public static IFlurlRequest AwsSign(this IFlurlRequest request, ImmutableCredentials credentials, RegionEndpoint regionEndpoint, object? data = null)
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

    public static async Task<IFlurlResponse> AwsSignAndPostJsonAsync(this IFlurlRequest request, ImmutableCredentials credentials, RegionEndpoint regionEndpoint, object? data = null) =>
        await request
            .AwsSign(credentials, regionEndpoint, data)
            .PostJsonAsync(data);
}