using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using AwsSignatureVersion4.Private;
using Flurl.Http;

namespace FiatChamp.Extensions;

public static class FlurlExtensions
{
    public static IFlurlRequest AwsSign(this IFlurlRequest request, ImmutableCredentials credentials, RegionEndpoint regionEndpoint, object? data = null) =>
        request.BeforeCall(call =>
        {
            var json = data == null ? "" : JsonSerializer.Serialize(data);
            call.HttpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
            Signer.Sign(call.HttpRequestMessage, null, new List<KeyValuePair<string, IEnumerable<string>>>(), DateTime.Now, regionEndpoint.SystemName, "execute-api", credentials);
        });
}