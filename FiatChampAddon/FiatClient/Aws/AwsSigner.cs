using System.Security.Cryptography;
using Amazon.Runtime.Internal.Auth;
using Amazon.Runtime;
using Amazon.Util;
using AwsSignatureVersion4.Private;
using System.Text;
using System.Web;
using FiatChamp.Extensions;

namespace FiatChamp.Aws
{
    public static class AwsSigner
    {
        public static Uri SignQuery(ImmutableCredentials credentials, string method, Uri uri, DateTime date, string regionName, string serviceName, string? content = null)
        {
            var credentialScope = $"{date.ToIso8601BasicDate()}/{regionName}/{serviceName}/{AWS4Signer.Terminator}";

            var query = HttpUtility.ParseQueryString(uri.Query).ToDictionary();
            query["X-Amz-Algorithm"] = "AWS4-HMAC-SHA256";
            query["X-Amz-Credential"] = $"{credentials.AccessKey}/{credentialScope}";
            query["X-Amz-Date"] = date.ToIso8601BasicDateTime();
            query["X-Amz-SignedHeaders"] = "host";

            var headers = new Dictionary<string, string>
            {
                { "host", uri.Host }
            };

            var contentHash = content is null ? "UNSIGNED-PAYLOAD" : AWSSDKUtils.ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(content)), true);
            var canonicalRequest = BuildCanonicalRequest(method, uri.AbsolutePath, query, headers, contentHash);
            var stringToSign = BuildStringToSign(date, credentialScope, canonicalRequest);

            var signingKey = AWS4Signer.ComposeSigningKey(credentials.SecretKey, regionName, date.ToIso8601BasicDate(), serviceName);
            var signatureHash = AWS4Signer.ComputeKeyedHash(SigningAlgorithm.HmacSHA256, signingKey, stringToSign);
            var signature = AWSSDKUtils.ToHex(signatureHash, true);

            query["X-Amz-Signature"] = signature;
            if (credentials.UseToken)
                query["X-Amz-Security-Token"] = credentials.Token;

            var builder = new UriBuilder(uri)
            {
                Query = string.Join('&', query.Select(i => AWSSDKUtils.UrlEncode(i.Key, false) + "=" + AWSSDKUtils.UrlEncode(i.Value, false)))
            };

            return builder.Uri;
        }

        public static string BuildCanonicalRequest(string method, string url, Dictionary<string, string> query, Dictionary<string, string> headers, string payloadHash)
        {
            query = query.ToDictionary(i => AWSSDKUtils.UrlEncode(i.Key, false), i => AWSSDKUtils.UrlEncode(i.Value, false));
            headers = headers.ToDictionary(i => i.Key.ToLower(), i => i.Value.Trim());

            var sb = new StringBuilder();

            sb.Append(method.ToUpper());
            sb.Append('\n');
            sb.Append(AWSSDKUtils.UrlEncode(url, true));
            sb.Append('\n');

            var first = true;
            foreach (var (key, value) in query.OrderBy(i => i.Key))
            {
                if (first)
                    first = false;
                else
                    sb.Append('&');
                sb.Append(key);
                sb.Append('=');
                sb.Append(value);
            }
            sb.Append('\n');

            foreach (var (key, value) in headers.OrderBy(i => i.Key))
            {
                sb.Append(key);
                sb.Append(':');
                sb.Append(value);
                sb.Append('\n');
            }
            sb.Append('\n');

            first = true;
            foreach (var (key, value) in headers.OrderBy(i => i.Key))
            {
                if (first)
                    first = false;
                else
                    sb.Append(';');
                sb.Append(key);
            }
            sb.Append('\n');

            sb.Append(payloadHash);

            return sb.ToString();
        }

        public static string BuildStringToSign(DateTime now, string credentialScope, string canonicalRequest)
        {
            var builder = new StringBuilder();

            builder.Append(AWS4Signer.AWS4AlgorithmTag);
            builder.Append('\n');

            builder.Append(now.ToIso8601BasicDateTime());
            builder.Append('\n');

            builder.Append(credentialScope);
            builder.Append('\n');

            builder.Append(AWSSDKUtils.ToHex(AWS4Signer.ComputeHash(canonicalRequest), true));

            return builder.ToString();
        }
    }
}
