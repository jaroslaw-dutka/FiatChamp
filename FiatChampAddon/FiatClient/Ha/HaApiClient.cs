using FiatChamp.Ha.Model;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;

namespace FiatChamp.Ha;

public class HaApiClient : IHaApiClient
{
    private readonly HaApiSettings _settings;
    private readonly IFlurlClient _flurlClient;

    public HaApiClient(IOptions<HaApiSettings> options, IFlurlClientCache flurlClientCache)
    {
        _settings = options.Value;
        _flurlClient = flurlClientCache.GetOrAdd("ha_api");
    }

    public async Task<HaConfig> GetConfigAsync() => await _flurlClient
        .Request(_settings.Url)
        .AppendPathSegment("config")
        .WithOAuthBearerToken(_settings.Token)
        .GetJsonAsync<HaConfig>();

    public async Task<IReadOnlyList<HaRestApiEntityState>> GetStatesAsync() => await _flurlClient
        .Request(_settings.Url)
        .AppendPathSegment("states")
        .WithOAuthBearerToken(_settings.Token)
        .GetJsonAsync<HaRestApiEntityState[]>();
}